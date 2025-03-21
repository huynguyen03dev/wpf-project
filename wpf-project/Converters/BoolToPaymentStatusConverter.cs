using System;
using System.Globalization;
using System.Windows.Data;

namespace wpf_project.Converters
{
    public class BoolToPaymentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPaid)
            {
                return isPaid ? "PAID" : "UNPAID";
            }
            return "UNKNOWN";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
