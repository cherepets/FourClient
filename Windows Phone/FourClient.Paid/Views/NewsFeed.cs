using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FourClient.Views
{
    public sealed partial class NewsFeed : UserControl, IBackButton
    {
        private const string SHOWN = "Shown400";

        private async void AfterLoad()
        {
            //Show Message
            var shown = ApplicationData.Current.LocalSettings.Values[SHOWN];
            if (shown == null || !(bool)shown)
            {
                await Task.Delay(5000);
                var dialog = new MessageDialog(
                    title: "Объявление",
                    content:
@"Привет! Спасибо за поддержку FourClient!

Это обновление служит для переноса ряда исправлений уже реализованных в версии для Windows 10 в версию для Windows Phone 8.1."
                    );
                ApplicationData.Current.LocalSettings.Values[SHOWN] = true;
                await dialog.ShowAsync();
            }
        }
    }
}
