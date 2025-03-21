using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wpf_project.Data;
using wpf_project.Models;

namespace wpf_project.Services
{
    public class OrderService
    {
        private readonly BookStoreContext _context;
        private readonly BookService _bookService;
        public string LastErrorMessage { get; private set; }

        public OrderService(BookStoreContext context, BookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task<bool> CreateOrder(int userId, List<OrderItem> items)
        {
            if (items == null || !items.Any())
            {
                LastErrorMessage = "No items in cart";
                return false;
            }

            try
            {
                // Thêm debug để xem lỗi chi tiết
                Console.WriteLine($"Creating order for user {userId} with {items.Count} items");

                // Step 1: Load books with direct SQL to ensure fresh data
                var bookIds = items.Select(i => i.BookId).ToList();
                var books = await _context.Books
                    .AsTracking() // Ensure tracking is enabled
                    .Where(b => bookIds.Contains(b.Id))
                    .ToDictionaryAsync(b => b.Id, b => b);

                // Step 2: Validate stock with logging
                foreach (var item in items)
                {
                    Console.WriteLine($"Checking book {item.BookId}, quantity {item.Quantity}");
                    if (!books.TryGetValue(item.BookId, out Book book))
                    {
                        LastErrorMessage = $"Book with ID {item.BookId} not found";
                        Console.WriteLine(LastErrorMessage);
                        return false;
                    }
                    
                    if (book.StockQuantity < item.Quantity)
                    {
                        LastErrorMessage = $"Insufficient stock for book {book.Title}: Requested {item.Quantity}, Available {book.StockQuantity}";
                        Console.WriteLine(LastErrorMessage);
                        return false;
                    }
                }

                // Step 3: Use explicit transaction
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Step 3.1: Create order record first
                        var order = new Order
                        {
                            UserId = userId,
                            OrderDate = DateTime.Now,
                            TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity),
                            IsPaid = false
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();
                        int orderId = order.Id;
                        Console.WriteLine($"Created order with ID {orderId}");

                        // Step 3.2: Create OrderItems directly 
                        foreach (var item in items)
                        {
                            // Create a new order item with clear references
                            var orderItem = new OrderItem
                            {
                                OrderId = orderId,
                                BookId = item.BookId,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice
                            };
                            _context.OrderItems.Add(orderItem);
                        }
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Added order items");

                        // Step 3.3: Update book stock
                        foreach (var item in items)
                        {
                            var book = books[item.BookId];
                            book.StockQuantity -= item.Quantity;
                        }
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Updated book stock");

                        // Commit the transaction
                        await transaction.CommitAsync();
                        Console.WriteLine("Transaction committed successfully");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        LastErrorMessage = $"Transaction error: {ex.Message}";
                        Console.WriteLine($"Order creation error: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = $"Database error: {ex.Message}";
                Console.WriteLine($"Database error during order creation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConfirmOrderPaymentAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return false;
                }

                order.IsPaid = true;
                order.PaymentDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error confirming order payment: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Order>> GetUserOrders(int userId)
        {
            // Đảm bảo tất cả dữ liệu được tải trước khi trả về kết quả
            var orders = await _context.Orders
                .AsNoTracking() // Sử dụng AsNoTracking để tránh theo dõi thay đổi
                .Where(o => o.UserId == userId)
                .ToListAsync();

            // Tải items và books cho từng order riêng biệt
            foreach (var order in orders)
            {
                var items = await _context.OrderItems
                    .AsNoTracking()
                    .Where(i => i.OrderId == order.Id)
                    .ToListAsync();

                foreach (var item in items)
                {
                    item.Book = await _context.Books
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.Id == item.BookId);
                }

                order.Items = items;
            }

            return orders;
        }

        public async Task<List<Order>> GetAllOrders()
        {
            try
            {
                // Log bắt đầu truy vấn
                Console.WriteLine("Starting to load all orders");

                // Đơn giản hóa truy vấn để tìm lỗi
                var orders = await _context.Orders.ToListAsync();
                Console.WriteLine($"Loaded {orders.Count} orders");

                // Thêm debug để xem dữ liệu
                foreach (var order in orders)
                {
                    Console.WriteLine($"Order ID: {order.Id}, UserID: {order.UserId}, Amount: {order.TotalAmount}, IsPaid: {order.IsPaid}");
                }

                // Tải OrderItems cho mỗi Order
                foreach (var order in orders)
                {
                    var items = await _context.OrderItems
                        .Where(oi => oi.OrderId == order.Id)
                        .ToListAsync();
                    Console.WriteLine($"Loaded {items.Count} items for order {order.Id}");

                    // Tải Book cho mỗi OrderItem
                    foreach (var item in items)
                    {
                        item.Book = await _context.Books
                            .FirstOrDefaultAsync(b => b.Id == item.BookId);
                    }

                    order.Items = items;
                }

                return orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllOrders: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error getting all orders: {ex.Message}");
                return new List<Order>();
            }
        }
    }
}
