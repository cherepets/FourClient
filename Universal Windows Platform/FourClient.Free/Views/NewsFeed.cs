using SOMAW81;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace FourClient.Views
{
    public partial class NewsFeed
    {
        private CoreDispatcher _dispatcher;
        private SomaAdViewer _ad;

        private void AfterLoad()
        {
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
