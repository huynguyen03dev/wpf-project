using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpf_project.Models;
using wpf_project.Services;
using wpf_project.Views;

namespace wpf_project
{
    public partial class MainWindow : Window
    {
        private readonly UserService _userService;

        public MainWindow()
        {
            InitializeComponent();
            _userService = ((App)Application.Current).GetService<UserService>();
            
            // Create default admin account if it doesn't exist
            _userService.EnsureAdminCreated().ConfigureAwait(false);
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
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