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
        private readonly User _currentUser;
        private readonly bool _passwordOnlyMode;

        public EditUser(UserService userService, User user, bool passwordOnlyMode = false)
        {
            InitializeComponent();
            _userService = userService;
            _currentUser = user;
            _passwordOnlyMode = passwordOnlyMode;
            
            LoadUserData();
            ConfigureUIForMode();
        }

        private void LoadUserData()
        {
            txtUsername.Text = _currentUser.Username;
            txtEmail.Text = _currentUser.Email;
            chkIsAdmin.IsChecked = _currentUser.IsAdmin;
        }

        private void ConfigureUIForMode()
        {
            if (_passwordOnlyMode)
            {
                this.Title = "Change Password";
                txtHeader.Text = "Change Your Password";
                
                txtUsername.IsEnabled = false;
                txtEmail.IsEnabled = false;
                chkIsAdmin.IsEnabled = false;
                
                txtPassword.Focus();
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_passwordOnlyMode)
            {
                if (string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("Please enter a new password.", "Password Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Passwords do not match.", "Password Mismatch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                try
                {
                    var updatedUser = new User
                    {
                        Id = _currentUser.Id,
                        Username = _currentUser.Username,
                        Email = _currentUser.Email,
                        IsAdmin = _currentUser.IsAdmin,
                        PasswordHash = _userService.HashPassword(txtPassword.Password)
                    };
                    
                    bool success = await UpdatePasswordOnly(updatedUser);
                    
                    if (success)
                    {
                        MessageBox.Show("Password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
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
        }

        private async Task<bool> UpdateUser(User user)
        {
            try
            {
                await _userService.UpdateUser(user);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task<bool> UpdatePasswordOnly(User user)
        {
            try
            {
                var existingUser = await _userService.GetUserById(user.Id);
                if (existingUser == null)
                    return false;
                    
                existingUser.PasswordHash = user.PasswordHash;
                
                return await _userService.UpdateUser(existingUser);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating password: {ex.Message}");
                throw;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
