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
                txtOrdersStatus.Text = "Loading orders...";
                
                // Load orders trong một scope riêng biệt
                List<Order> orders = null;
                
                try
                {
                    // Sử dụng Task.Run để đảm bảo truy vấn chạy trên thread riêng biệt
                    orders = await Task.Run(async () => await _orderService.GetAllOrders());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception getting orders: {ex.Message}");
                    orders = new List<Order>();
                }
                
                // Debug output
                System.Diagnostics.Debug.WriteLine($"AdminDashboard: Received {orders?.Count ?? 0} orders from service");
                
                if (orders != null && orders.Any())
                {
                    // Đảm bảo không có null references
                    foreach (var order in orders)
                    {
                        if (order.Items == null)
                            order.Items = new List<OrderItem>();
                    }
                    
                    // Hiển thị thông tin orders
                    txtOrdersStatus.Text = $"Found {orders.Count} orders.";
                    
                    // Cập nhật UI thông qua dispatcher để tránh thread conflicts
                    this.Dispatcher.Invoke(() => {
                        dgOrders.ItemsSource = null;
                        dgOrders.ItemsSource = orders;
                    });
                }
                else
                {
                    txtOrdersStatus.Text = "No orders found in database.";
                    dgOrders.ItemsSource = new List<Order>();
                    
                    // Hiển thị thông tin debug bổ sung
                    System.Diagnostics.Debug.WriteLine("No orders to display or empty orders list returned");
                }
            }
            catch (Exception ex)
            {
                txtOrdersStatus.Text = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadOrdersAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
