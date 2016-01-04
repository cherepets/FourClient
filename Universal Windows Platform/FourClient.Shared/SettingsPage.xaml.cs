using System;
using System.Linq;
using Windows.Foundation;
using FourClient.Extensions;
using FourClient.UserControls;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using FourClient.HtmlRender;
using Windows.UI.ViewManagement;

namespace FourClient
{
    public sealed partial class SettingsPage
    {
        public PageHeaderBase PageHeader;

        public SettingsPage()
        {
            InitializeComponent();
            RebuildUI();
            Load();
        }

        private bool _loaded;

        private async void RebuildUI()
        {
            if (SettingsService.IsPhone)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                StatusBar.GetForCurrentView().ForegroundColor = SettingsService.GetStatusForeground();
                await StatusBar.GetForCurrentView().ShowAsync();
            }
        }

        private void Load()
        {
            MainThemeToggle.IsOn = SettingsService.MainTheme;
            ArticleThemeToggle.IsOn = SettingsService.ArticleTheme;
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
            RenderBox.Items?.Clear();
            HtmlRenderFactory.Renders.ForEach(r => RenderBox.Items?.Add(new ComboBoxItem { Content = r }));
            var render = RenderBox.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == SettingsService.Render);
            if (render != null)
                RenderBox.SelectedValue = render;
            var youtube = YouTubeBox.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == SettingsService.YouTube);
            if (youtube != null)
                YouTubeBox.SelectedValue = youtube;
            _loaded = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;
            RequestedTheme = SettingsService.GetMainTheme();
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            View.Animate();
        }

        private void MainThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetMainTheme(MainThemeToggle.IsOn);
            RequestedTheme = SettingsService.GetMainTheme();
            if (SettingsService.IsPhone)
                StatusBar.GetForCurrentView().ForegroundColor = SettingsService.GetStatusForeground();
        }

        private void ArticleThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;
            SettingsService.SetArticleTheme(ArticleThemeToggle.IsOn);
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
                var dialog = new MessageDialog("Удаление завершено", "Всё готово!");
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await dialog.ShowAsync());
            }
            catch
            {
                // ok
            }
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingsPage_BackRequested;
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }
    }
}