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

Ну, теперь то точно последнее обновление для WP8.1.

- Настройки выравнивания текста

- Кнопка 'поделиться' внутри статьи

До встречи на Windows 10 :)"
                    );
                ApplicationData.Current.LocalSettings.Values[SHOWN] = true;
                await dialog.ShowAsync();
            }
        }
    }
}
