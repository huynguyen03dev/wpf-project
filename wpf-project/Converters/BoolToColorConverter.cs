using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace wpf_project.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPaid)
            {
                return isPaid ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
