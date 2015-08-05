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
