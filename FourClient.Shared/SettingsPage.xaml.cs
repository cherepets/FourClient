using FourClient.UserControls;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Controls;

namespace FourClient
{
    public sealed partial class SettingsPage : Page
    {
        public PageHeaderBase PageHeader;

        public SettingsPage()
        {
            this.InitializeComponent();
            RebuildUI();
        }

        private void RebuildUI()
        {
            PageHeader = new StatusBar();
            PageHeaderGrid.Children.Clear();
            PageHeaderGrid.Children.Add(PageHeader);
            PageHeader.SetTitle("Настройки");
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            RequestedTheme = SettingsService.MainTheme;
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            this.Frame.GoBack();
        }
    }
}