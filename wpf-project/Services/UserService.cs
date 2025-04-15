using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using wpf_project.Data;
using wpf_project.Models;

namespace wpf_project.Services
{
    public class UserService
    {
        private readonly BookStoreContext _context;

        public UserService(BookStoreContext context)
        {
            _context = context;
        }

        public async Task<User> AuthenticateUser(string username, string password)
        {
            try
            {
                string hashedPassword = HashPassword(password);
                return await _context.Users
                    .FirstOrDefaultAsync(u => 
                        u.Username.ToLower() == username.ToLower() && 
                        u.PasswordHash == hashedPassword);
            }
            catch (Exception)
            {
                // Handle database errors gracefully
                return null;
            }
        }

        public async Task<bool> RegisterUser(string username, string password, string email, bool isAdmin = false)
        {
            try
            {
                // Make sure the Users table exists
                await EnsureDatabaseTablesExist();

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
                    return false;

                User newUser = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password),
                    Email = email,
                    IsAdmin = isAdmin,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task EnsureAdminCreated()
        {
            try
            {
                // Make sure the Users table exists
                await EnsureDatabaseTablesExist();

                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.IsAdmin == true);
                    
                if (adminUser == null)
                {
                    await RegisterUser("admin", "admin123", "admin@bookstore.com", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating admin user: {ex.Message}");
            }
        }

        private async Task EnsureDatabaseTablesExist()
        {
            // Check if database exists, create it if it doesn't
            await _context.Database.EnsureCreatedAsync();
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<User> GetUserById(int userId) {
            try {
                return await _context.Users.FindAsync(userId);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Error getting user by ID: {ex.Message}");
                return null;
            }
        }
        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<bool> DeleteUser(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // Kiểm tra xem user có đơn hàng hay không
                    var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == userId);
                    if (hasOrders)
                    {
                        throw new InvalidOperationException("Cannot delete user with existing orders.");
                    }

                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                throw;
            }
        }

        // Thêm phương thức UpdateUser
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                // Chỉ cập nhật các thuộc tính cần thiết
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                    return false;
                    
                existingUser.IsAdmin = user.IsAdmin;
                // Không cập nhật password vì nó đã được mã hoá
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating user: {ex.Message}");
                throw;
            }
        }
    }
}
