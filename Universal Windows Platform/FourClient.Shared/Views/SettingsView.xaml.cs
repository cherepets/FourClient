using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public sealed partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
            Loaded += SettingsView_Loaded;
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            AppThemeToggle.IsOn = Settings.Current.AppTheme;
            ArticleThemeToggle.IsOn = Settings.Current.ArticleTheme;
            LiveTileBox.IsChecked = Settings.Current.LiveTile;
            SetComboBoxValue(YouTubeBox, Settings.Current.YouTube);
            SetComboBoxValue(AlignBox, Settings.Current.Align);
            SetComboBoxValue(FontBox, Settings.Current.FontFace);
            FontSlider.Value = Settings.Current.FontSize;
            FontSliderCap.Text = FontSlider.Value.ToString();
            CacheSlider.Value = Settings.Current.CacheDays;
            CacheSliderCap.Text = CacheSlider.Value.ToString();
            VersionBlock.Text = $"v. {Settings.Current.DisplayVersion}";
            _init = true;
        }

        private bool _init;

        private void AppThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.AppTheme = AppThemeToggle.IsOn;
        }
        private void ArticleThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.ArticleTheme = ArticleThemeToggle.IsOn;
        }

        private void LiveTileBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.LiveTile = LiveTileBox.IsChecked ?? true;
        }

        private void YouTubeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.YouTube = GetComboBoxValue(YouTubeBox);
        }

        private void AlignBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.Align = GetComboBoxValue(AlignBox);
        }

        private void FontBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.FontFace = GetComboBoxValue(FontBox);
        }

        private void FontSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.FontSize = (int)FontSlider.Value;
            FontSliderCap.Text = FontSlider.Value.ToString();
        }

        private void CacheSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.CacheDays = (int)CacheSlider.Value;
            CacheSliderCap.Text = CacheSlider.Value.ToString();
        }

        private void SetComboBoxValue(ComboBox box, string value)
        {
            var item = box.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == value);
            if (item != null) box.SelectedValue = item;
        }

        private string GetComboBoxValue(ComboBox box)
        {
            return (((ComboBoxItem)box.SelectedItem).Content as string);
        }
    }
}
