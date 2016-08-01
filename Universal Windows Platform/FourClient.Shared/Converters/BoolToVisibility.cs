using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FourClient.Converters
{
    public class BoolToVisibility : IValueConverter
    {

        public object Convert(object value, Type type, object parameter, string language)
        {
            var val = System.Convert.ToBoolean(value);
            {
                val = !val;
            }

            if (val)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
