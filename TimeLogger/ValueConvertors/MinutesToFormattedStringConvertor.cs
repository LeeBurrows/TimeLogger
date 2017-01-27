using System;
using System.Windows;
using System.Windows.Data;
using TimeLogger.Helpers;

namespace TimeLogger.ValueConvertors
{
    [ValueConversion(typeof(int), typeof(string))]
    public class MinutesToFormattedStringConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val = 0;
            try
            {
                val = int.Parse(value.ToString());
            }
            catch { }
            return FormattingUtils.formatReportDuration(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
