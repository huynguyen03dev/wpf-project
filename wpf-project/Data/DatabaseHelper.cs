using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using wpf_project.Models;

namespace wpf_project.Data
{
    public static class DatabaseHelper
    {
        public static async Task<bool> UpdateDatabaseSchema(BookStoreContext context)
        {
            try
            {
                // Check if SchemaVersion table exists first
                bool hasSchemaTable = await TableExists(context, "SchemaVersion");
                
                if (!hasSchemaTable)
                {
                    // Create schema version table and add columns to Orders table
                    await CreateSchemaVersionTable(context);
                    
                    // Check if Orders table needs updating
                    if (await TableExists(context, "Orders"))
                    {
                        bool hasIsPaidColumn = await ColumnExists(context, "Orders", "IsPaid");
                        bool hasPaymentDateColumn = await ColumnExists(context, "Orders", "PaymentDate");
                        
                        if (!hasIsPaidColumn || !hasPaymentDateColumn)
                        {
                            await AddMissingColumnsToOrders(context);
                        }
                    }
                    
                    // Set initial schema version
                    await SetSchemaVersion(context, BookStoreContext.SchemaVersion);
                    return true;
                }
                else
                {
                    // Table exists, check version
                    int currentVersion = await GetSchemaVersion(context);
                    if (currentVersion < BookStoreContext.SchemaVersion)
                    {
                        // Schema needs updating
                        if (currentVersion < 2)
                        {
                            await AddMissingColumnsToOrders(context);
                        }
                        
                        await SetSchemaVersion(context, BookStoreContext.SchemaVersion);
                        return true;
                    }
                }
                
                return false; // No update needed
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating database schema: {ex.Message}");
                MessageBox.Show($"Error updating database schema: {ex.Message}\n\nThe application may not function correctly.",
                    "Database Schema Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        private static async Task<bool> HasSchemaVersionTable(BookStoreContext context)
        {
            return await TableExists(context, "SchemaVersion");
        }
        
        private static async Task<bool> TableExists(BookStoreContext context, string tableName)
        {
            try
            {
                // Sử dụng cách kiểm tra an toàn hơn thông qua SQL Server system tables
                var result = await context.Database.SqlQueryRaw<int>(
                    $"SELECT COUNT(*) FROM sys.tables WHERE name = '{tableName}'").ToListAsync();
                    
                return result != null && result.Count > 0 && result[0] > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if table exists: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> ColumnExists(BookStoreContext context, string tableName, string columnName)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    $"SELECT TOP 1 {columnName} FROM {tableName}");
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static async Task CreateSchemaVersionTable(BookStoreContext context)
        {
            await context.Database.ExecuteSqlRawAsync(
                @"CREATE TABLE SchemaVersion (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Version INT NOT NULL,
                    UpdatedDate DATETIME NOT NULL
                )");
        }
        
        private static async Task AddMissingColumnsToOrders(BookStoreContext context)
        {
            try
            {
                // Add IsPaid column if it doesn't exist
                if (!await ColumnExists(context, "Orders", "IsPaid"))
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE Orders ADD IsPaid BIT NOT NULL DEFAULT 0");
                }
                
                // Add PaymentDate column if it doesn't exist
                if (!await ColumnExists(context, "Orders", "PaymentDate"))
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE Orders ADD PaymentDate DATETIME NULL");
                }
                
                System.Diagnostics.Debug.WriteLine("Added missing columns to Orders table");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding columns to Orders table: {ex.Message}");
                throw;
            }
        }
        
        private static async Task<int> GetSchemaVersion(BookStoreContext context)
        {
            try
            {
                // Kiểm tra trực tiếp xem bảng có tồn tại không
                var tableExists = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) FROM sys.tables WHERE name = 'SchemaVersion'").ToListAsync();
                    
                if (tableExists == null || tableExists.Count == 0 || tableExists[0] == 0)
                {
                    return 0; // Bảng không tồn tại
                }
                
                // Bảng tồn tại, giờ đọc version
                var versionResult = await context.Database.SqlQueryRaw<int>(
                    "SELECT TOP 1 Version FROM SchemaVersion ORDER BY Id DESC").ToListAsync();
                
                if (versionResult != null && versionResult.Count > 0)
                {
                    return versionResult[0];
                }
                
                return 0; // Không tìm thấy version
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting schema version: {ex.Message}");
                return 0; // Mặc định về 0 nếu có lỗi
            }
        }
        
        private static async Task SetSchemaVersion(BookStoreContext context, int version)
        {
            await context.Database.ExecuteSqlRawAsync(
                $"INSERT INTO SchemaVersion (Version, UpdatedDate) VALUES ({version}, GETDATE())");
        }
    }
}
