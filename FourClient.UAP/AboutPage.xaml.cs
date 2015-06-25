using FourClient.UserControls;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace FourClient
{
    public sealed partial class AboutPage : Page
    {
        public PageHeaderBase PageHeader;

        public AboutPage()
        {
            this.InitializeComponent();
            RebuildUI();
        }

        private void RebuildUI()
        {
            PageHeader = new StatusBar();
            PageHeaderGrid.Children.Clear();
            PageHeaderGrid.Children.Add(PageHeader);
            PageHeader.SetTitle("О приложении");

            DisplayInformation.AutoRotationPreferences = SettingsService.LargeScreen ?
                DisplayOrientations.LandscapeFlipped | DisplayOrientations.Portrait | DisplayOrientations.Landscape :
                DisplayOrientations.Portrait;
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += AboutPage_BackRequested;
            RequestedTheme = SettingsService.MainTheme;
        }

        private void AboutPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            this.Frame.GoBack();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= AboutPage_BackRequested;
        }
    }
}
