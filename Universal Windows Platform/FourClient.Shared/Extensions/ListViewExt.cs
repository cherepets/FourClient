using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FourClient.Extensions
{
    internal static class ListViewExt
    {
        public static ScrollViewer GetScrollViewer(this ListViewBase input)
            => (ScrollViewer)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(input, 0), 0);
    }
}
