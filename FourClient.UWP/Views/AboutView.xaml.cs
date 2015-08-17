using System;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public sealed partial class AboutView : UserControl
    {
        public AboutView()
        {
            this.InitializeComponent();
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
    }
}
