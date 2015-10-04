using FourClient.HtmlRender;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public sealed partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            Load();
        }

        private bool _loaded;

        private void Load()
        {
            LiveTileBox.IsChecked = SettingsService.LiveTile;
            RenderSwitchBox.IsChecked = SettingsService.RenderSwitch;
            UpperMenuBox.IsOn = SettingsService.UpperMenu;
            Slider.Value = SettingsService.FontSize;
            var face = FontBox.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == SettingsService.FontFace);
            if (face != null)
                FontBox.SelectedItem = face;
            var align = AlignBox.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == SettingsService.Align);
            if (align != null)
                AlignBox.SelectedItem = align;
            RenderBox.Items.Clear();
            HtmlRenderFactory.Renders.ForEach(r => RenderBox.Items.Add(new ComboBoxItem { Content = r }));
            var render = RenderBox.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == SettingsService.Render);
            if (render != null)
                RenderBox.SelectedValue = render;
            var youtube = YouTubeBox.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == SettingsService.YouTube);
            if (youtube != null)
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

        private void AlignBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetAlign(((ComboBoxItem)AlignBox.SelectedItem).Content as string);
        }

        private void YouTubeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetYouTube(((ComboBoxItem)YouTubeBox.SelectedItem).Content as string);
        }

        private void RenderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetRender(((ComboBoxItem)RenderBox.SelectedItem).Content as string);
        }

        private void LiveTileBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetLiveTile(LiveTileBox.IsChecked.Value);
        }

        private void RenderSwitchBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetRenderSwitch(RenderSwitchBox.IsChecked.Value);
        }

        private void UpperMenuBox_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetUpperMenu(UpperMenuBox.IsOn);
        }

        private void CacheButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var appData = ApplicationData.Current.TemporaryFolder;
            var operation = appData.DeleteAsync(StorageDeleteOption.PermanentDelete);
            operation.Completed += Operation_Completed;
        }

        private async void Operation_Completed(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
            try
            {
                var coreDispatcher = Window.Current.Dispatcher;
                var dialog = new MessageDialog("Удаление завершено", "Всё готово!");
                await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await dialog.ShowAsync());
            }
            catch
            {
                // ok
            }
        }
    }
}
