using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FourClient.UserControls
{
    public sealed partial class HoverListView : ListView
    {
        public HoverListView()
        {
            Loaded += HoverListView_Loaded;
            RequestedTheme = ElementTheme.Light;
        }

        private void HoverListView_Loaded(object sender, RoutedEventArgs e)
        {
            BorderBrush = Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
            BorderThickness = SettingsService.IsPhone ? new Thickness(0, 1, 0, 1) : new Thickness(1);
            Background = Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush;
        }
    }
}
