using System.Windows;
using wpf_project.Models;

namespace wpf_project.Views
{
    public partial class BookDetails : Window
    {
        private readonly Book _book;
        public bool AddedToCart { get; private set; }

        public BookDetails(Book book)
        {
            InitializeComponent();
            _book = book;
            LoadBookDetails();
        }

        private void LoadBookDetails()
        {
            txtBookTitle.Text = _book.Title;
            txtAuthor.Text = _book.Author;
            txtGenre.Text = _book.Genre;
            txtISBN.Text = _book.ISBN;
            txtPrice.Text = $"{_book.Price:C}";
            txtStock.Text = _book.StockQuantity.ToString();
            txtDescription.Text = _book.Description;

            // Disable the "Add to Cart" button if the book is out of stock
            btnAddToCart.IsEnabled = _book.StockQuantity > 0;
            if (!btnAddToCart.IsEnabled)
            {
                btnAddToCart.Content = "Out of Stock";
                btnAddToCart.Background = System.Windows.Media.Brushes.LightGray;
            }
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            AddedToCart = true;
            DialogResult = true;
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
