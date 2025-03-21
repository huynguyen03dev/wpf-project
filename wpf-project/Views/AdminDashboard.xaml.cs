using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using wpf_project.Models;
using wpf_project.Services;
using wpf_project.Views;

namespace wpf_project.Views
{
    public partial class AdminDashboard : Window
    {
        private readonly BookService _bookService;
        private readonly OrderService _orderService;

        public AdminDashboard()
        {
            InitializeComponent();
            
            try
            {
                _bookService = ((App)Application.Current).GetService<BookService>();
                _orderService = ((App)Application.Current).GetService<OrderService>();

                LoadBooksAsync();
                LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing AdminDashboard: {ex.Message}", 
                               "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadBooksAsync()
        {
            try
            {
                var books = await _bookService.GetAllBooks();
                // Materialize the data before setting it to the DataGrid
                dgBooks.ItemsSource = books.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadOrdersAsync()
        {
            try
            {
                // Xử lý trực tiếp trên UI thread để tìm lỗi
                List<Order> orders = await _orderService.GetAllOrders();
                
                // Debug output
                Console.WriteLine($"Loaded {orders?.Count ?? 0} orders for admin dashboard");
                
                if (orders != null && orders.Any())
                {
                    dgOrders.ItemsSource = orders;
                }
                else
                {
                    Console.WriteLine("No orders found or orders list is null");
                    dgOrders.ItemsSource = new List<Order>(); // Set empty list to avoid null reference
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in LoadOrdersAsync: {ex.Message}");
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        private void btnAddBook_Click(object sender, RoutedEventArgs e)
        {
            EditBook addBookWindow = new EditBook(null);
            if (addBookWindow.ShowDialog() == true)
            {
                LoadBooksAsync();
            }
        }

        private void btnEditBook_Click(object sender, RoutedEventArgs e)
        {
            if (dgBooks.SelectedItem is Book selectedBook)
            {
                EditBook editBookWindow = new EditBook(selectedBook);
                if (editBookWindow.ShowDialog() == true)
                {
                    LoadBooksAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select a book to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (dgBooks.SelectedItem is Book selectedBook)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete \"{selectedBook.Title}\"?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _bookService.DeleteBook(selectedBook.Id);
                    LoadBooksAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select a book to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnSearchBooks_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = txtSearchBooks.Text.Trim().ToLower();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadBooksAsync();
                return;
            }

            var allBooks = await _bookService.GetAllBooks();
            var filteredBooks = allBooks.Where(b => 
                b.Title.ToLower().Contains(searchTerm) || 
                b.Author.ToLower().Contains(searchTerm) ||
                b.Genre.ToLower().Contains(searchTerm) ||
                b.ISBN.ToLower().Contains(searchTerm)
            ).ToList();

            dgBooks.ItemsSource = filteredBooks;
        }

        private void btnViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                OrderDetails orderDetails = new OrderDetails(selectedOrder);
                bool? result = orderDetails.ShowDialog();
                
                // Refresh orders list if changes were made
                if (result == true)
                {
                    LoadOrdersAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select an order to view details.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
