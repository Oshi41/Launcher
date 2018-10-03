using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MaterialDesignThemes.Wpf.Converters;

namespace Launcher.Converters
{
    class StrToVisConverter : IValueConverter
    {
        private readonly IValueConverter _boolToVisConverter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return _boolToVisConverter.Convert(string.IsNullOrEmpty(str), targetType, parameter, culture);
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
