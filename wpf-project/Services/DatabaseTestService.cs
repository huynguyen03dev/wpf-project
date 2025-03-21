using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using wpf_project.Data;
using wpf_project.Models;

namespace wpf_project.Services
{
    public class DatabaseTestService
    {
        private readonly BookStoreContext _dbContext;

        public DatabaseTestService(BookStoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsConnected, string Message)> TestConnectionAsync()
        {
            try
            {
                // Attempt to connect to the database
                bool canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    return (true, "Successfully connected to the database!");
                }
                else
                {
                    return (false, "Could not connect to the database. Database may not exist or connection string is incorrect.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Database connection error: {ex.Message}");
            }
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            try
            {
                // Ensure the database is created
                bool created = await _dbContext.Database.EnsureCreatedAsync();
                if (created)
                {
                    // Database was just created, let's verify the tables were created correctly
                    await VerifyTablesCreatedAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating database: {ex.Message}", "Database Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ApplyMigrationsAsync()
        {
            try
            {
                // Check if the database exists first
                bool exists = await _dbContext.Database.CanConnectAsync();

                if (exists)
                {
                    try
                    {
                        // Check if there are any pending migrations
                        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Any())
                        {
                            // Apply pending migrations
                            await _dbContext.Database.MigrateAsync();
                        }
                    }
                    catch (Exception)
                    {
                        // If we can't check for migrations (may happen if database doesn't support migrations)
                        // Try to create the database schema directly
                        await _dbContext.Database.EnsureCreatedAsync();
                        await VerifyTablesCreatedAsync();
                    }
                }
                else
                {
                    // Database doesn't exist yet, create it
                    await _dbContext.Database.EnsureCreatedAsync();
                    await VerifyTablesCreatedAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying migrations: {ex.Message}", "Migration Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task VerifyTablesCreatedAsync()
        {
            // Query for any existing tables to verify they exist
            try
            {
                // Try to read from the Users table
                var anyUser = await _dbContext.Users.AnyAsync();
                
                // This will throw an exception if the Books table doesn't exist
                var anyBook = await _dbContext.Books.AnyAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error verifying database tables: {ex.Message}\n\n" +
                    "This could be due to incorrect table names or permissions.", 
                    "Database Structure Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
