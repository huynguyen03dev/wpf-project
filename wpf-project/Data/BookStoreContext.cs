using Microsoft.EntityFrameworkCore;
using wpf_project.Models;

namespace wpf_project.Data
{
    public class BookStoreContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public BookStoreContext(DbContextOptions<BookStoreContext> options) : base(options)
        {
            // Set to track all entities for better performance
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Prevent locking if not using MARS
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<User>()
                .ToTable("Users");

            // Configure Book entity
            modelBuilder.Entity<Book>()
                .HasKey(b => b.Id);
            modelBuilder.Entity<Book>()
                .ToTable("Books");

            // Configure Order entity
            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);
            modelBuilder.Entity<Order>()
                .ToTable("Orders");

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.BookId });
            modelBuilder.Entity<OrderItem>()
                .ToTable("OrderItems");

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Book)
                .WithMany()
                .HasForeignKey(oi => oi.BookId);
        }
    }
}
