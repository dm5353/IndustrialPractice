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

            var dateOnly = date.Date;
            var ru = new CultureInfo("ru-RU");

            if (dateOnly == today)
                return "Сегодня";

            if (dateOnly == yesterday)
                return "Вчера";

            string format = dateOnly.Year != today.Year
                ? "MMMM yyyy"
                : "d MMMM";

            string text = dateOnly.ToString(format, ru);
            text = CapitalizeFirstLetter(text);
            return text;
        }

        string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetter(input[i]))
                {
                    return input[..i] + char.ToUpper(input[i]) + input[(i + 1)..];
                }
            }

            return input;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}