using System.Globalization;
using System.Windows.Data;

namespace MyFinance.Converters
{
    public class FutureOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date > DateTime.Now ? 0.4 : 1.0;

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
