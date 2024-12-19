using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Project.Converters
{
    public class StringToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var stringValue = value as string;
            var parameterValue = parameter as string;

            if (parameter == null)
            {
                return string.IsNullOrEmpty(stringValue) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (string.IsNullOrEmpty(stringValue) || string.IsNullOrEmpty(parameterValue))
            {
                return Visibility.Collapsed;
            }
            return stringValue == parameterValue ? Visibility.Visible : Visibility.Collapsed;

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
