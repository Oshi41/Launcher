using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using Launcher.Helper;

namespace SettingsHelper.Converter
{
    [ContentProperty("Cases")]
    public class CaseConverter : IValueConverter
    {
        public object DefaultValue { get; set; }

        public IList Cases { get; set; }

        public CaseConverter()
        {
            Cases = new List<Case>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Cases.IsNullOrEmpty())
                return DefaultValue;

            var key = value?.ToString() ?? string.Empty;
            var find = Cases.OfType<Case>().FirstOrDefault(x => string.Equals(key, x.If));
            return find == null
                ? DefaultValue
                : find.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ContentProperty("Value")]
    public class Case
    {
        public string If { get; set; }
        public object Value { get; set; }

        public Case()
        {

        }

        public Case(object condition, object value)
        {
            If = condition?.ToString() ?? string.Empty;
            Value = value;
        }

        public override string ToString()
        {
            return $"If={If}, than - {Value}";
        }
    }
}
