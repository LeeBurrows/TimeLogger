using System;
using System.Windows;
using System.Windows.Data;
using TimeLogger.DTO;
using TimeLogger.Models;

namespace TimeLogger.ValueConvertors
{
    [ValueConversion(typeof(int), typeof(string))]
    public class TagIdToLabelConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val = 0;
            try
            {
                val = int.Parse(value.ToString());
            }
            catch { }
            Tag tag = Model.instance.getTagByID(val);
            return (tag != null) ? tag.label : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
