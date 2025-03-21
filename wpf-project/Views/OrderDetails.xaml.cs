using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using wpf_project.Models;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class OrderDetails : Window
    {
        private readonly Order _order;
        private readonly OrderService _orderService;
        private bool _orderUpdated = false;

        public bool OrderUpdated => _orderUpdated;

        public OrderDetails(Order order)
        {
            InitializeComponent();
            _order = order;
            _orderService = ((App)Application.Current).GetService<OrderService>();

            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            // Set order header information
            txtOrderHeader.Text = $"Order #{_order.Id}";
            txtOrderDate.Text = $"Order Date: {_order.OrderDate:dd/MM/yyyy HH:mm}";
            txtCustomer.Text = $"Customer ID: {_order.UserId}";

            // Set payment status
            UpdatePaymentStatus();

            // Load order items with calculated subtotals
            var orderItemsWithSubtotal = _order.Items.Select(item => new 
            {
                item.BookId,
                BookTitle = item.Book?.Title ?? $"Book ID: {item.BookId}",
                item.UnitPrice,
                item.Quantity,
                Subtotal = item.UnitPrice * item.Quantity
            }).ToList();

            dgOrderItems.ItemsSource = orderItemsWithSubtotal;

            // Set total amount
            txtTotalAmount.Text = $"{_order.TotalAmount:C}";
        }

        private void UpdatePaymentStatus()
        {
            if (_order.IsPaid)
            {
                txtPaymentStatus.Text = "PAID";
                txtPaymentStatus.Foreground = new SolidColorBrush(Colors.Green);
                txtPaymentDate.Text = $"(Paid on {_order.PaymentDate:dd/MM/yyyy HH:mm})";
                txtPaymentDate.Visibility = Visibility.Visible;
                btnConfirmPayment.IsEnabled = false;
                btnConfirmPayment.Opacity = 0.5;
            }
            else
            {
                txtPaymentStatus.Text = "UNPAID";
                txtPaymentStatus.Foreground = new SolidColorBrush(Colors.Red);
                txtPaymentDate.Visibility = Visibility.Collapsed;
                btnConfirmPayment.IsEnabled = true;
                btnConfirmPayment.Opacity = 1;
            }
        }

        private async void btnConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                $"Confirm that payment for Order #{_order.Id} has been received?",
                "Confirm Payment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = await _orderService.ConfirmOrderPaymentAsync(_order.Id);
                
                if (success)
                {
                    _order.IsPaid = true;
                    _order.PaymentDate = DateTime.Now;
                    UpdatePaymentStatus();
                    
                    MessageBox.Show(
                        $"Payment for Order #{_order.Id} has been confirmed.",
                        "Payment Confirmed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    _orderUpdated = true;
                }
                else
                {
                    MessageBox.Show(
                        "There was an error confirming the payment. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _orderUpdated;
            Close();
        }
    }
}
