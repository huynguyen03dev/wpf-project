using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using wpf_project.Services;

namespace wpf_project.Views
{
    public partial class DatabaseStatusWindow : Window
    {
        private readonly DatabaseTestService _databaseTestService;

        public DatabaseStatusWindow()
        {
            InitializeComponent();
            
            _databaseTestService = ((App)Application.Current).GetService<DatabaseTestService>();
            
            TestConnectionAsync();
        }

        private async void TestConnectionAsync()
        {
            txtConnectionStatus.Text = "Testing connection...";
            connectionIndicator.Fill = Brushes.Gray;
            txtConnectionDetails.Text = "Attempting to connect to the database...";
            btnRetryConnection.IsEnabled = false;

            await Task.Delay(500); // Small delay for UI feedback

            var result = await _databaseTestService.TestConnectionAsync();

            if (result.IsConnected)
            {
                txtConnectionStatus.Text = "Connected";
                connectionIndicator.Fill = Brushes.Green;
                txtConnectionDetails.Text = result.Message + "\n\nThe application is ready to use.";
            }
            else
            {
                txtConnectionStatus.Text = "Connection Failed";
                connectionIndicator.Fill = Brushes.Red;
                txtConnectionDetails.Text = result.Message + "\n\nPossible solutions:\n" +
                    "1. Verify SQL Server is running\n" +
                    "2. Check the connection string\n" +
                    "3. Ensure the database exists or can be created\n" +
                    "4. Verify user permissions";
            }

            btnRetryConnection.IsEnabled = true;
        }

        private void btnRetryConnection_Click(object sender, RoutedEventArgs e)
        {
            TestConnectionAsync();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
