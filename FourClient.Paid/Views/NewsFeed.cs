using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FourClient.Views
{
    public sealed partial class NewsFeed : UserControl, IBackButton
    {
        private const string SHOWN = "Shown300";

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

Немного об интересностях новой версии:

- Обработка новостей теперь осуществляется двумя сервисами - один для платной версии, один для бесплатной

- Много визуальных изменений: нижняя панель с анимированными значками, например :)

- Сохранение истории чтения и локальный кэш

- Можно закрыть статью жестом (от левой рамки)

- Поправлены некоторые ошибки"
                    );
                ApplicationData.Current.LocalSettings.Values[SHOWN] = true;
                await dialog.ShowAsync();
            }
        }
    }
}