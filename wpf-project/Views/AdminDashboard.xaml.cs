using Microsoft.EntityFrameworkCore;
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
        private readonly UserService _userService;

        public AdminDashboard()
        {
            InitializeComponent();
            
            try
            {
                _bookService = ((App)Application.Current).GetService<BookService>();
                _orderService = ((App)Application.Current).GetService<OrderService>();
                _userService = ((App)Application.Current).GetService<UserService>(); // 
                LoadBooksAsync();
                LoadOrdersAsync();
                LoadUsersAsync();
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

        // Thêm phương thức LoadUsersAsync
        private async void LoadUsersAsync()
        {
            try
            {
                txtUsersStatus.Text = "Loading users...";
                
                var users = await _userService.GetAllUsers();
                
                if (users != null && users.Any())
                {
                    txtUsersStatus.Text = $"Found {users.Count} users.";
                    
                    this.Dispatcher.Invoke(() => {
                        dgUsers.ItemsSource = null;
                        dgUsers.ItemsSource = users;
                    });
                }
                else
                {
                    txtUsersStatus.Text = "No users found.";
                    dgUsers.ItemsSource = new List<User>();
                }
            }
            catch (Exception ex)
            {
                txtUsersStatus.Text = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadUsersAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thêm phương thức btnDeleteUser_Click
        private async void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                // Kiểm tra nếu đang xoá tài khoản của mình
                if (user.Id == ((App)Application.Current).CurrentUser.Id)
                {
                    MessageBox.Show("You cannot delete your own account.", 
                        "Operation Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete user \"{user.Username}\"?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _userService.DeleteUser(user.Id);
                        MessageBox.Show($"User \"{user.Username}\" deleted successfully.", 
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsersAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting user: {ex.Message}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Thêm phương thức btnToggleAdmin_Click
        private async void btnToggleAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                // Không cho phép bỏ quyền admin của chính mình
                if (user.Id == ((App)Application.Current).CurrentUser.Id && user.IsAdmin)
                {
                    MessageBox.Show("You cannot remove admin rights from your own account.", 
                        "Operation Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    // Toggle trạng thái IsAdmin
                    user.IsAdmin = !user.IsAdmin;
                    await _userService.UpdateUser(user);
                    
                    string statusText = user.IsAdmin ? "granted" : "removed";
                    MessageBox.Show($"Admin privileges {statusText} for user \"{user.Username}\".", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating user: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Thêm phương thức btnRefreshUsers_Click
        private void btnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsersAsync();
        }

        private void adminTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Kiểm tra tab được chọn
            //if (adminTabControl.SelectedItem is TabItem selectedTab) {
            //    // Nếu chuyển sang tab Users, tải dữ liệu users
            //    if (selectedTab == usersTab) {
            //        LoadUsersAsync();
            //    }
            //    // Có thể thêm các tab khác nếu cần
            //    else if (selectedTab.Header.ToString() == "Orders") {
            //        LoadOrdersAsync();
            //    } else if (selectedTab.Header.ToString() == "Books") {
            //        LoadBooksAsync();
            //    }
            //}
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e) {
            if (sender is Button button && button.DataContext is User selectedUser) {
                try {
                    // Tạo cửa sổ EditUser và truyền thông tin người dùng được chọn
                    EditUser editUserWindow = new EditUser(_userService, selectedUser);

                    // Hiển thị cửa sổ dưới dạng dialog
                    bool? result = editUserWindow.ShowDialog();

                    // Nếu đã cập nhật thành công (DialogResult = true), làm mới danh sách
                    if (result == true) {
                        // Làm mới danh sách người dùng
                        LoadUsersAsync();
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Error opening user editor: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
