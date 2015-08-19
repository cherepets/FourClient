using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FourClient.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type type, object parameter, string language)
        {
            if (!(value is bool)) return null;
            var val = (bool)value;
            return val 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
