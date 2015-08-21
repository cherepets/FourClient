using FourClient.UserControls;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FourClient
{
    public sealed partial class SettingsPage : Page
    {
        public PageHeaderBase PageHeader;

        public SettingsPage()
        {
            InitializeComponent();
            RebuildUI();
        }

        private void RebuildUI()
        {
            if (SettingsService.IsPhone)
            {
                PageHeader = new MobileHeader();
                PageHeaderGrid.Children.Clear();
                PageHeaderGrid.Children.Add(PageHeader);
                PageHeader.SetTitle("Настройки");
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;
            RequestedTheme = SettingsService.MainTheme;
            if (SettingsService.IsPhone)
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView()?.HideAsync();
            }
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) => Frame.GoBack();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingsPage_BackRequested;
        }
    }
}