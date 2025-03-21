using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wpf_project.Data;
using wpf_project.Models;

namespace wpf_project.Services
{
    public class BookService
    {
        private readonly BookStoreContext _context;

        public BookService(BookStoreContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllBooks()
        {
            try
            {
                return await _context.Books.ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting books: {ex.Message}");
                return new List<Book>();
            }
        }

        public async Task<Book> GetBookById(int id)
        {
            try
            {
                return await _context.Books.FindAsync(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting book by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Book> AddBook(Book book)
        {
            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return book;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding book: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateBook(Book book)
        {
            try
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating book: {ex.Message}");
            }
        }

        public async Task DeleteBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book != null)
                {
                    _context.Books.Remove(book);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting book: {ex.Message}");
            }
        }

        public async Task<bool> UpdateStock(int bookId, int quantity)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null || book.StockQuantity < quantity)
                    return false;

                book.StockQuantity -= quantity;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stock: {ex.Message}");
                return false;
            }
        }

        public async Task AddSampleBooksIfEmpty()
        {
            try
            {
                if (!await _context.Books.AnyAsync())
                {
                    await _context.Books.AddRangeAsync(new List<Book>
                    {
                        new Book
                        {
                            Title = "Clean Code",
                            Author = "Robert C. Martin",
                            ISBN = "9780132350884",
                            Price = 44.99m,
                            StockQuantity = 10,
                            Description = "A handbook of agile software craftsmanship",
                            Genre = "Programming"
                        },
                        new Book
                        {
                            Title = "The Great Gatsby",
                            Author = "F. Scott Fitzgerald",
                            ISBN = "9780743273565",
                            Price = 14.99m,
                            StockQuantity = 15,
                            Description = "A novel about the mysterious Jay Gatsby",
                            Genre = "Fiction"
                        }
                    });
                    
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding sample books: {ex.Message}");
            }
        }
    }
}
