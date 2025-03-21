using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using wpf_project.Models;
using wpf_project.Services;
using System.Windows.Input;

namespace wpf_project.Views
{
    public partial class ShoppingCart : Window
    {
        private ObservableCollection<CartItem> _cartItems;
        private decimal _totalAmount;
        private readonly OrderService _orderService;
        private readonly BookService _bookService;
        private readonly User _currentUser;

        public ShoppingCart(List<Book> books)
        {
            InitializeComponent();
            _bookService = ((App)Application.Current).GetService<BookService>();
            _orderService = ((App)Application.Current).GetService<OrderService>();
            _currentUser = ((App)Application.Current).CurrentUser;

            // Create cart items from books
            _cartItems = new ObservableCollection<CartItem>();
            
            // Only add items if there are books
            if (books != null && books.Any())
            {
                // Group books by Id to combine duplicates
                var groupedBooks = books.GroupBy(b => b.Id);
                foreach (var group in groupedBooks)
                {
                    var book = group.First();
                    _cartItems.Add(new CartItem
                    {
                        BookId = book.Id,
                        Title = book.Title,
                        Author = book.Author,
                        Price = book.Price,
                        Quantity = group.Count()
                    });
                }
            }

            // Important: Set ItemsSource before calling UpdateCartDisplay
            dgCartItems.ItemsSource = _cartItems;
            UpdateCartDisplay();
        }

        private void UpdateCartDisplay()
        {
            // Show empty cart message if cart is empty
            if (_cartItems == null || _cartItems.Count == 0)
            {
                txtEmptyCart.Visibility = Visibility.Visible;
                dgCartItems.Visibility = Visibility.Collapsed;
                btnCheckout.IsEnabled = false;
            }
            else
            {
                txtEmptyCart.Visibility = Visibility.Collapsed;
                dgCartItems.Visibility = Visibility.Visible;
                btnCheckout.IsEnabled = true;
            }

            CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            if (_cartItems != null && _cartItems.Any())
            {
                _totalAmount = _cartItems.Sum(item => item.Price * item.Quantity);
            }
            else
            {
                _totalAmount = 0;
            }
            txtTotalAmount.Text = $"{_totalAmount:C}";
        }

        private async void btnIncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem item)
            {
                try {
                    // Get fresh book data from the database
                    Book book = await _bookService.GetBookById(item.BookId);
                    
                    if (book != null && book.StockQuantity > item.Quantity)
                    {
                        item.Quantity++;
                        CalculateTotalAmount();
                    }
                    else
                    {
                        MessageBox.Show($"Cannot add more items. Maximum stock reached ({book?.StockQuantity ?? 0}).", 
                            "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show($"Error checking stock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnDecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    CalculateTotalAmount();
                }
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItem item)
            {
                _cartItems.Remove(item);
                UpdateCartDisplay();
            }
        }

        private async void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable the button to prevent multiple clicks
                btnCheckout.IsEnabled = false;
                
                if (_cartItems == null || !_cartItems.Any())
                {
                    MessageBox.Show("Your cart is empty.", "Checkout", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnCheckout.IsEnabled = true;
                    return;
                }

                // Verify stock one more time before checkout
                bool insufficientStock = false;
                string outOfStockItems = "";
                
                foreach (var item in _cartItems)
                {
                    var book = await _bookService.GetBookById(item.BookId);
                    if (book == null || book.StockQuantity < item.Quantity)
                    {
                        insufficientStock = true;
                        outOfStockItems += $"\n- {item.Title}: Requested {item.Quantity}, Available {book?.StockQuantity ?? 0}";
                    }
                }
                
                if (insufficientStock)
                {
                    MessageBox.Show($"Some items in your cart are out of stock or have insufficient quantity:{outOfStockItems}", 
                        "Stock Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnCheckout.IsEnabled = true;
                    return;
                }

                // Create order items from cart items
                List<OrderItem> orderItems = _cartItems.Select(item => new OrderItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                }).ToList();

                // Create the order with progress indicator
                Mouse.OverrideCursor = Cursors.Wait;
                bool success = await _orderService.CreateOrder(_currentUser.Id, orderItems);
                Mouse.OverrideCursor = null;

                if (success)
                {
                    MessageBox.Show($"Order placed successfully! Total amount: {_totalAmount:C}", 
                        "Order Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Clear cart and close
                    _cartItems.Clear();
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    string errorMessage = _orderService.LastErrorMessage ?? "Unknown error";
                    MessageBox.Show($"There was an error processing your order.\nDetails: {errorMessage}", 
                        "Order Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnCheckout.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during checkout: {ex.Message}", "Checkout Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnCheckout.IsEnabled = true;
            }
        }

        private void btnContinueShopping_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    // Helper class for cart items with INotifyPropertyChanged to update UI
    public class CartItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
