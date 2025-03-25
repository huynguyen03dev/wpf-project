using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using wpf_project.Data;
using wpf_project.Models;
using wpf_project.Services;
using wpf_project.Views;

namespace wpf_project
{
    public partial class App : Application
    {
        public User CurrentUser { get; set; }
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();

            // Initialize database
            InitializeDatabaseAsync();
        }

        private async void InitializeDatabaseAsync()
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    // Get database test service
                    var dbTestService = scope.ServiceProvider.GetRequiredService<DatabaseTestService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<BookStoreContext>();
                    
                    // Test the connection
                    var (isConnected, message) = await dbTestService.TestConnectionAsync();
                    
                    if (!isConnected)
                    {
                        MessageBox.Show($"Database connection failed: {message}\n\nThe application will try to create the database.", 
                            "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    
                    try
                    {
                        // Ensure database exists
                        bool dbCreated = await dbContext.Database.EnsureCreatedAsync();
                        
                        if (dbCreated)
                        {
                            // Database was just created, initialize data
                            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                            await userService.EnsureAdminCreated();

                            var bookService = scope.ServiceProvider.GetRequiredService<BookService>();
                            await bookService.AddSampleBooksIfEmpty();
                        }
                        else
                        {
                            // Database already exists, check if schema needs updates
                            bool schemaUpdated = await DatabaseHelper.UpdateDatabaseSchema(dbContext);
                            if (schemaUpdated)
                            {
                                System.Diagnostics.Debug.WriteLine("Database schema was updated");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error initializing database: {ex.Message}", 
                            "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application initialization error: {ex.Message}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Add DbContext with explicit connection string with MARS enabled
            services.AddDbContext<BookStoreContext>(options =>
            {
                options.UseSqlServer("Server=Huy-Nitro\\NOADB;Database=book-store-db;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;",
                    sqlServerOptions => 
                    {
                        // Set command timeout through the SQL Server options builder
                        sqlServerOptions.CommandTimeout(60);
                    });
                
                // Enable detailed errors for debugging
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                
            }, ServiceLifetime.Transient); // Use Transient to avoid shared DbContext

            // Add database test service
            services.AddTransient<DatabaseTestService>();

            // Add services
            services.AddTransient<UserService>();
            services.AddTransient<BookService>();
            services.AddTransient<OrderService>();
        }

        public T GetService<T>() where T : class
        {
            return serviceProvider.GetService<T>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Start with login window
            var loginWindow = new Login();
            loginWindow.Show();
        }
    }
}
