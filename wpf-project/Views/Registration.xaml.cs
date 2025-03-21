using System.Windows;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class Registration : Window
    {
        private readonly UserService _userService;

        public Registration()
        {
            InitializeComponent();
            _userService = ((App)Application.Current).GetService<UserService>();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Validation
            if (string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Try to register the user
            bool success = await _userService.RegisterUser(username, password, email);

            if (success)
            {
                MessageBox.Show("Registration successful! You can now log in.", "Registration Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Username already exists. Please choose another one.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
