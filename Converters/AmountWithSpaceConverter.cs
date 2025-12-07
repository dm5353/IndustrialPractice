using System.Globalization;
using System.Windows.Data;

namespace MyFinance.Converters
{
    public class AmountWithSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal dec)
                return string.Format(new CultureInfo("ru-RU"), "{0:N2} ₽", dec);
            if (value is double dbl)
                return string.Format(new CultureInfo("ru-RU"), "{0:N2} ₽", dbl);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}