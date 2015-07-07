using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FourClient.Views
{
    public sealed partial class NewsFeed : UserControl, IBackButton
    {
        private const string SHOWN = "Shown370";

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

Хочу вам сообщить, что эта версия (3.7) - предположительно последняя для Windows Phone 8.1 и в следующий раз мы увидимся уже на Windows 10 (и настольной тоже ☺ ).

Еще хочу напомнить что FourClient - приложение с полностью открытым исходным кодом, доступным на GitHub, в разработке которого может принять участие любой желающий."
                    );
                ApplicationData.Current.LocalSettings.Values[SHOWN] = true;
                await dialog.ShowAsync();
            }
        }
    }
}