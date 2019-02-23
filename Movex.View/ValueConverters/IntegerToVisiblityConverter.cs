using System;
using System.Globalization;
using System.Windows;

namespace Movex.View
{
    /// <summary>
    /// A converter that takes in a boolean and returns a <see cref="Visibility"/>
    /// </summary>
    public class IntegerToVisiblityConverter : BaseValueConverter<IntegerToVisiblityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (int)value!=0 ? Visibility.Hidden : Visibility.Visible;
            else
                return (int)value!=0 ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
