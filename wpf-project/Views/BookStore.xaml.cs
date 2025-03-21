using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using wpf_project.Models;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class BookStore : Window
    {
        private readonly BookService _bookService;
        private readonly User _currentUser;
        private List<Book> _allBooks;
        private List<Book> _cartItems = new List<Book>();

        public BookStore()
        {
            InitializeComponent();
            _bookService = ((App)Application.Current).GetService<BookService>();
            _currentUser = ((App)Application.Current).CurrentUser;
            
            txtWelcome.Text = $"Welcome, {_currentUser.Username}";
            
            LoadBooksAsync();
        }

        private async void LoadBooksAsync()
        {
            _allBooks = await _bookService.GetAllBooks();
            lstBooks.ItemsSource = _allBooks;
            LoadGenres();
        }

        private void LoadGenres()
        {
            var genres = _allBooks.Select(b => b.Genre).Distinct().ToList();
            genres.Insert(0, "All Genres");
            cmbGenre.ItemsSource = genres;
            cmbGenre.SelectedIndex = 0;
        }

        private void FilterBooks()
        {
            var filteredBooks = _allBooks.ToList();
            
            // Filter by genre
            if (cmbGenre.SelectedItem != null && cmbGenre.SelectedItem.ToString() != "All Genres")
            {
                filteredBooks = filteredBooks.Where(b => b.Genre == cmbGenre.SelectedItem.ToString()).ToList();
            }
            
            // Filter by price
            if (decimal.TryParse(txtMinPrice.Text, out decimal minPrice))
            {
                filteredBooks = filteredBooks.Where(b => b.Price >= minPrice).ToList();
            }
            
            if (decimal.TryParse(txtMaxPrice.Text, out decimal maxPrice))
            {
                filteredBooks = filteredBooks.Where(b => b.Price <= maxPrice).ToList();
            }
            
            lstBooks.ItemsSource = filteredBooks;
        }

        private void cmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterBooks();
        }

        private void PriceFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBooks();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            cmbGenre.SelectedIndex = 0;
            txtMinPrice.Text = string.Empty;
            txtMaxPrice.Text = string.Empty;
            lstBooks.ItemsSource = _allBooks;
        }

        private void lstBooks_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstBooks.SelectedItem is Book selectedBook)
            {
                BookDetails bookDetails = new BookDetails(selectedBook);
                if (bookDetails.ShowDialog() == true)
                {
                    // Book was added to cart
                    _cartItems.Add(selectedBook);
                    MessageBox.Show($"\"{selectedBook.Title}\" has been added to your cart.", "Added to Cart", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnViewCart_Click(object sender, RoutedEventArgs e)
        {
            ShoppingCart cart = new ShoppingCart(_cartItems);
            cart.ShowDialog();
            
            // Refresh book list after checkout (to update stock)
            LoadBooksAsync();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }
    }
}
