using System;
using System.Windows;
using System.Windows.Data;

namespace Launcher.Converters
{
    public class EnumBooleanConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Equals(value, parameter))
                return GetInvertedValue(true);

            if (Equals(value, null) || Equals(parameter, null))
            {
                return GetInvertedValue(false);
            }

            return GetInvertedValue(string.Equals(value.ToString(), parameter.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Срабатываем только если выставили true
            if (value is bool equals 
                && parameter != null)
            {
                var realVal = GetInvertedValue(equals);

                if (!realVal)
                    return null;

                return parameter;
            }

            return DependencyProperty.UnsetValue;
        }
        #endregion

        private bool GetInvertedValue(bool value)
        {
            return !(!value | IsInverted);
        }
    }
}
