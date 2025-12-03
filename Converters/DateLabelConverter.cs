using System.Globalization;
using System.Windows.Data;

namespace MyFinance.Converters
{
    public class DateLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime date)
                return value;

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            var dateOnly = date.Date; // убираем время

            if (dateOnly == today)
                return "Сегодня";

            if (dateOnly == yesterday)
                return "Вчера";

            // Формат: 5 января
            return dateOnly.ToString("d MMMM", new CultureInfo("ru-RU"));
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
