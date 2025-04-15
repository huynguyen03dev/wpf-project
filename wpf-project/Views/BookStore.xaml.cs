using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using wpf_project.Models;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class BookStore : Window
    {
        private readonly BookService _bookService;
        private readonly UserService _userService; // Thêm UserService
        private readonly User _currentUser;
        private List<Book> _allBooks = new List<Book>(); // Initialize with empty list
        private List<Book> _filteredBooks = new List<Book>(); // Initialize with empty list
        private List<Book> _cartItems = new List<Book>();

        public BookStore()
        {
            InitializeComponent();
            
            // Set up service references
            _bookService = ((App)Application.Current).GetService<BookService>();
            _userService = ((App)Application.Current).GetService<UserService>(); // Thêm UserService
            _currentUser = ((App)Application.Current).CurrentUser;
            
            txtWelcome.Text = $"Welcome, {_currentUser?.Username ?? "User"}";
            
            // Thêm event handler cho việc nhấp vào tên người dùng
            txtWelcome.MouseLeftButtonUp += txtWelcome_MouseLeftButtonUp;
            
            // Initialize default sorting
            cmbSort.SelectedIndex = 0;
            
            // Instead of calling LoadBooksAsync directly, add it to the Loaded event
            this.Loaded += (s, e) => 
            {
                LoadBooksAsync();
            };
        }

        private async void LoadBooksAsync()
        {
            try
            {
                _allBooks = await _bookService.GetAllBooks() ?? new List<Book>();
                _filteredBooks = new List<Book>(_allBooks);
                
                // Update display
                UpdateBookDisplay();
                LoadGenres();
                
                // Set up price filter slider max value based on highest book price
                if (_allBooks.Any())
                {
                    decimal maxPrice = _allBooks.Max(b => b.Price);
                    sliderPrice.Maximum = (double)Math.Ceiling(Math.Min(maxPrice * 1.2m, 500)); // 20% above max price or 500, whichever is lower
                    
                    // Update price range text
                    txtPriceRange.Text = $"$0 - ${sliderPrice.Maximum}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGenres()
        {
            if (_allBooks == null)
            {
                _allBooks = new List<Book>();
            }

            var genres = _allBooks.Select(b => b.Genre).Where(g => !string.IsNullOrEmpty(g)).Distinct().OrderBy(g => g).ToList();
            genres.Insert(0, "All Genres");
            cmbGenre.ItemsSource = genres;
            cmbGenre.SelectedIndex = 0;
        }

        private void UpdateBookDisplay()
        {
            // Add null check for booksItemsControl
            if (booksItemsControl == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: booksItemsControl is null in UpdateBookDisplay");
                return;
            }

            if (_filteredBooks == null)
            {
                _filteredBooks = new List<Book>();
            }

            // Use Dispatcher to ensure UI updates happen on UI thread
            this.Dispatcher.Invoke(() =>
            {
                booksItemsControl.ItemsSource = _filteredBooks;
                
                // Update result count text
                if (_filteredBooks.Count == (_allBooks?.Count ?? 0))
                {
                    txtResultCount.Text = $"Showing all {_filteredBooks.Count} books";
                }
                else
                {
                    txtResultCount.Text = $"Showing {_filteredBooks.Count} of {_allBooks?.Count ?? 0} books";
                }
                
                // Show or hide no results message
                txtNoResults.Visibility = _filteredBooks.Any() ? Visibility.Collapsed : Visibility.Visible;
            });
        }

        private void ApplyFilters()
        {
            if (_allBooks == null)
            {
                _allBooks = new List<Book>();
            }

            _filteredBooks = new List<Book>(_allBooks);
            
            // Apply genre filter
            if (cmbGenre.SelectedItem != null && cmbGenre.SelectedItem.ToString() != "All Genres")
            {
                string selectedGenre = cmbGenre.SelectedItem.ToString();
                _filteredBooks = _filteredBooks.Where(b => b.Genre == selectedGenre).ToList();
            }
            
            // Apply price filter
            if (decimal.TryParse(txtMinPrice.Text, out decimal minPrice))
            {
                _filteredBooks = _filteredBooks.Where(b => b.Price >= minPrice).ToList();
            }
            
            if (decimal.TryParse(txtMaxPrice.Text, out decimal maxPrice))
            {
                _filteredBooks = _filteredBooks.Where(b => b.Price <= maxPrice).ToList();
            }
            
            // Apply in-stock filter
            if (chkInStock.IsChecked == true)
            {
                _filteredBooks = _filteredBooks.Where(b => b.StockQuantity > 0).ToList();
            }
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchTerm = txtSearch.Text.ToLower();
                _filteredBooks = _filteredBooks.Where(b => 
                    b.Title.ToLower().Contains(searchTerm) || 
                    b.Author.ToLower().Contains(searchTerm) ||
                    b.Description?.ToLower().Contains(searchTerm) == true ||
                    b.Genre.ToLower().Contains(searchTerm)
                ).ToList();
            }
            
            // Apply sorting
            ApplySorting();
            
            // Update display
            UpdateBookDisplay();
        }

        private void ApplySorting()
        {
            if (_filteredBooks == null || !_filteredBooks.Any())
                return;
                
            switch (cmbSort.SelectedIndex)
            {
                case 1: // Price: Low to High
                    _filteredBooks = _filteredBooks.OrderBy(b => b.Price).ToList();
                    break;
                case 2: // Price: High to Low
                    _filteredBooks = _filteredBooks.OrderByDescending(b => b.Price).ToList();
                    break;
                case 3: // Title: A to Z
                    _filteredBooks = _filteredBooks.OrderBy(b => b.Title).ToList();
                    break;
                default: // Default sorting (keep original order)
                    break;
            }
        }

        private void cmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void PriceFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void sliderPrice_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtMaxPrice.Text = Math.Ceiling(sliderPrice.Value).ToString();
            ApplyFilters();
        }

        private void chkInStock_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            cmbGenre.SelectedIndex = 0;
            txtMinPrice.Text = string.Empty;
            txtMaxPrice.Text = string.Empty;
            txtSearch.Text = string.Empty;
            chkInStock.IsChecked = false;
            cmbSort.SelectedIndex = 0;
            
            _filteredBooks = new List<Book>(_allBooks);
            UpdateBookDisplay();
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySorting();
            UpdateBookDisplay();
        }

        private void QuickFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagValue)
            {
                // Reset filters first
                btnClearFilters_Click(null, null);
                
                switch (tagValue)
                {
                    case "All":
                        // All books (filters already cleared)
                        break;
                    case "Fiction":
                    case "Programming":
                        // Set genre filter
                        int genreIndex = ((List<string>)cmbGenre.ItemsSource).IndexOf(tagValue);
                        if (genreIndex >= 0)
                            cmbGenre.SelectedIndex = genreIndex;
                        break;
                    case "Bestsellers":
                        // Show books with stock < 5 (assuming these are bestsellers that sell quickly)
                        _filteredBooks = _allBooks.Where(b => b.StockQuantity > 0 && b.StockQuantity < 5).ToList();
                        UpdateBookDisplay();
                        break;
                }
            }
        }

        private void Book_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Book selectedBook)
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
            if (cart.ShowDialog() == true)
            {
                // Cart was checked out, reload books to update stock
                LoadBooksAsync();
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        // Xử lý sự kiện khi người dùng nhấn vào tên người dùng
        private void txtWelcome_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Mở cửa sổ EditUser ở chế độ chỉ thay đổi mật khẩu
                EditUser editUserWindow = new EditUser(_userService, _currentUser, true);
                
                // Hiển thị cửa sổ dưới dạng dialog
                bool? result = editUserWindow.ShowDialog();
                
                // Không cần làm gì thêm khi đóng dialog vì thông tin người dùng
                // đã được cập nhật trong cơ sở dữ liệu
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
