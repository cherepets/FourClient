using System;
using Windows.ApplicationModel.Store;
using FourClient.UserControls;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using FourClient.Extensions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

namespace FourClient
{
    public sealed partial class AboutPage
    {
        public PageHeaderBase PageHeader;

        public AboutPage()
        {
            InitializeComponent();
            RebuildUI();
        }

        private async void RebuildUI()
        {
            if (SettingsService.IsPhone)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                StatusBar.GetForCurrentView().ForegroundColor = SettingsService.GetStatusForeground();
                await StatusBar.GetForCurrentView().ShowAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += AboutPage_BackRequested;
            RequestedTheme = SettingsService.GetMainTheme();
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            View.Animate();
        }

        private async void Email_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var mailto = new Uri("mailto:?to=cherepets@live.ru&subject=FourClient app");
            await Launcher.LaunchUriAsync(mailto);
        }

        private async void StoreBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        private async void VkBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://m.vk.com/fourvk"));
        }

        private void AboutPage_BackRequested(object sender, BackRequestedEventArgs e)
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
            SystemNavigationManager.GetForCurrentView().BackRequested -= AboutPage_BackRequested;
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }
    }
}
