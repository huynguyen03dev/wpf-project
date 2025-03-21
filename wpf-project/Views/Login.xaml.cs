using System;
using System.Windows;
using wpf_project.Models;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class Login : Window
    {
        private readonly UserService _userService;

        public Login()
        {
            InitializeComponent();
            _userService = ((App)Application.Current).GetService<UserService>();
            
            // Create default admin account if it doesn't exist
            try
            {
                _userService.EnsureAdminCreated().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Login initialization: {ex.Message}");
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = txtUsername.Text;
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                User user = await _userService.AuthenticateUser(username, password);

                if (user != null)
                {
                    // Store current user in App-level data
                    ((App)Application.Current).CurrentUser = user;
                    
                    if (user.IsAdmin)
                    {
                        AdminDashboard adminDashboard = new AdminDashboard();
                        adminDashboard.Show();
                    }
                    else
                    {
                        BookStore bookStore = new BookStore();
                        bookStore.Show();
                    }
                    
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lnkRegister_Click(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.ShowDialog();
        }

        private void CheckDatabaseConnection_Click(object sender, RoutedEventArgs e)
        {
            DatabaseStatusWindow statusWindow = new DatabaseStatusWindow();
            statusWindow.ShowDialog();
        }
    }
}