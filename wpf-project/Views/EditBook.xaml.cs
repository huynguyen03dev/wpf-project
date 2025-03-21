using Microsoft.Win32;
using System;
using System.Windows;
using wpf_project.Models;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class EditBook : Window
    {
        private readonly BookService _bookService;
        private Book _book;
        private bool _isNewBook;

        public EditBook(Book book)
        {
            InitializeComponent();
            _bookService = ((App)Application.Current).GetService<BookService>();
            
            if (book == null)
            {
                // Adding a new book
                _book = new Book();
                _isNewBook = true;
                txtHeader.Text = "Add New Book";
            }
            else
            {
                // Editing an existing book
                _book = book;
                _isNewBook = false;
                txtHeader.Text = $"Edit Book: {_book.Title}";
                LoadBookData();
            }
        }

        private void LoadBookData()
        {
            txtTitle.Text = _book.Title;
            txtAuthor.Text = _book.Author;
            txtISBN.Text = _book.ISBN;
            txtGenre.Text = _book.Genre;
            txtPrice.Text = _book.Price.ToString();
            txtStock.Text = _book.StockQuantity.ToString();
            txtDescription.Text = _book.Description;
            txtImagePath.Text = _book.ImagePath;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png",
                Title = "Select Book Cover Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtImagePath.Text = openFileDialog.FileName;
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || 
                string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                string.IsNullOrWhiteSpace(txtISBN.Text) ||
                string.IsNullOrWhiteSpace(txtGenre.Text))
            {
                MessageBox.Show("Title, Author, ISBN, and Genre are required fields.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price greater than zero.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Please enter a valid stock quantity (zero or greater).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Update book object
            _book.Title = txtTitle.Text;
            _book.Author = txtAuthor.Text;
            _book.ISBN = txtISBN.Text;
            _book.Genre = txtGenre.Text;
            _book.Price = price;
            _book.StockQuantity = stock;
            _book.Description = txtDescription.Text;
            _book.ImagePath = txtImagePath.Text;

            // Save book
            if (_isNewBook)
            {
                await _bookService.AddBook(_book);
                MessageBox.Show("Book added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _bookService.UpdateBook(_book);
                MessageBox.Show("Book updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
