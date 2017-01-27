using System;
using System.Windows;
using System.Windows.Data;
using TimeLogger.Helpers;

namespace TimeLogger.ValueConvertors
{
    [ValueConversion(typeof(string), typeof(string))]
    public class RemoveNewLinesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                string val = (string)value;
                return FormattingUtils.removeNewlines(val);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
