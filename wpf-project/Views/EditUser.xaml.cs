using System;
using System.Threading.Tasks;
using System.Windows;
using wpf_project.Data;
using wpf_project.Models;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class EditUser : Window
    {
        private readonly UserService _userService;
        private readonly BookStoreContext _context;
        private readonly User _currentUser;

        public EditUser(UserService userService, BookStoreContext context, User user)
        {
            InitializeComponent();
            _userService = userService;
            _context = context;
            _currentUser = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            txtUsername.Text = _currentUser.Username;
            txtEmail.Text = _currentUser.Email;
            chkIsAdmin.IsChecked = _currentUser.IsAdmin;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _currentUser.Email = txtEmail.Text;
            _currentUser.IsAdmin = chkIsAdmin.IsChecked ?? false;

            if (!string.IsNullOrEmpty(txtPassword.Password))
            {
                _currentUser.PasswordHash = _userService.HashPassword(txtPassword.Password);
            }

            bool success = await UpdateUser(_currentUser);

            if (success)
            {
                MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to update user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> UpdateUser(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
