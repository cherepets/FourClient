using SOMAWP81;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FourClient.Views
{
    public sealed partial class NewsFeed : UserControl, IBackButton
    {
        private const string SHOWN = "Shown300";
        private CoreDispatcher _dispatcher;
        private SomaAdViewer _ad;

        private async void AfterLoad()
        {
            //Add ad block
            _ad = new SomaAdViewer
            {
                Adspace = 65855648,
                Pub = 923884844
            };
            AdGrid.Children.Add(_ad);
            _dispatcher = Dispatcher;
            _ad.AdError += mediator_AdMediatorError;
            _ad.NewAdAvailable += mediator_AdMediatorFilled;
            _ad.StartAds();
            //Show Message
            var shown = ApplicationData.Current.LocalSettings.Values[SHOWN];
            if (shown == null || !(bool)shown)
            {
                await Task.Delay(5000);
                var dialog = new MessageDialog(
                    title: "Объявление",
                    content:
@"Привет! Спасибо за использование FourClient!

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

        private async void mediator_AdMediatorFilled(object sender, EventArgs e)
        {
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        PaidGrid.Visibility = Visibility.Collapsed;
                        _ad.Visibility = Visibility.Visible;
                    });
            }
            finally
            {
                PaidGrid.Children.Clear();
            }
        }

        private async void mediator_AdMediatorError(object sender, string ErrorCode, string ErrorDescription)
        {
            try
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        PaidGrid.Visibility = Visibility.Visible;
                        _ad.Visibility = Visibility.Collapsed;
                    });
                _ad.StopAds();
            }
            finally
            {
                AdGrid.Children.Clear();
            }
        }
    }
}