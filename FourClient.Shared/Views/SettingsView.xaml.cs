using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public sealed partial class SettingsView : UserControl
    {
        public SettingsView(
            )
        {
            this.InitializeComponent();
            Load();
        }

        private bool _loaded;

        private void Load()
        {
            LiveTileBox.IsChecked = SettingsService.LiveTile;
            Slider.Value = SettingsService.FontSize;
            var face = FontBox.Items.First(c => ((ComboBoxItem)c).Content as string == SettingsService.FontFace);
            FontBox.SelectedItem = face;
            var youtube = YouTubeBox.Items.First(c => ((ComboBoxItem)c).Content as string == SettingsService.YouTube);
            YouTubeBox.SelectedValue = youtube;
            _loaded = true;
        }

        private void SetMainTheme(ElementTheme theme)
        {
            RequestedTheme = theme;
            SettingsService.SetMainTheme(theme);
        }
        
        private void Rectangle1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SetMainTheme(ElementTheme.Default);
        }

        private void Rectangle2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SetMainTheme(ElementTheme.Dark);
        }

        private void Rectangle3_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SetMainTheme(ElementTheme.Light);
        }

        private void Rectangle4_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetArticleTheme(ElementTheme.Default);
        }

        private void Rectangle5_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetArticleTheme(ElementTheme.Dark);
        }

        private void Rectangle6_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetArticleTheme(ElementTheme.Light);
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetFontSize((int)e.NewValue);
        }

        private void FontBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetFontFace(((ComboBoxItem)FontBox.SelectedItem).Content as string);
        }

        private void YouTubeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetYouTube(((ComboBoxItem)YouTubeBox.SelectedItem).Content as string);
        }

        private void LiveTileBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetLiveTile(LiveTileBox.IsChecked.Value);
        }
    }
}
