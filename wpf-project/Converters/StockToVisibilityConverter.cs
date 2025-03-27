using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace wpf_project.Converters
{
    public class StockToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is int stockQuantity)
                {
                    return stockQuantity <= 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (int.TryParse(value?.ToString(), out int parsedStock))
                {
                    return parsedStock <= 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in StockToVisibilityConverter: {ex.Message}");
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
