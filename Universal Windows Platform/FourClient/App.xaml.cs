using FourClient.Extensions;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FourClient
{
    public sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            Current.UnhandledException += Current_UnhandledException;
        }

        private async void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var dialog = new MessageDialog(e.Message);
            await dialog.ShowAsync();
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!SettingsService.IsPhone)
            {
                var accent = (Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush).Color;
                var light = accent.Lighten();
                var dark = accent.Darken();
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = accent;
                titleBar.ForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = accent;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = dark;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = light;
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonInactiveBackgroundColor = accent;
                titleBar.ButtonInactiveForegroundColor = Colors.White;
                titleBar.InactiveBackgroundColor = dark;
                titleBar.InactiveForegroundColor = Colors.White;
            }
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                rootFrame.ContentTransitions = null;
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            try
            {
                var argString = e.Arguments;
                if (!string.IsNullOrEmpty(argString))
                {
                    var tiles = await SecondaryTile.FindAllForPackageAsync();
                    var tile = tiles.FirstOrDefault(t => t.TileId == e.TileId);
                    var title = tile == null ? "FourClient" : tile.DisplayName;
                    var args = argString.Split(';').ToList();
                    while (args.Count() > 2)
                    {
                        args[1] += ';' + args[2];
                        args.Remove(args[2]);
                    }
                    switch (args.Count())
                    {
                        case 1:
                            MainPage.GoToNewsFeed(args[0]);
                            break;
                        case 2:
                            MainPage.GoToArticle(args[0], title, args[1], null, null);
                            break;
                    }
                }
            }
            finally
            {
                Window.Current.Activate();
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                MainPage.SaveState();
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnResuming(object sender, object e)
        {
            try
            {
                var article = (string)ApplicationData.Current.LocalSettings.Values["SuspendedArticle"];
                if (article != null)
                {
                    var args = article.Split(';');
                    var title = (string)ApplicationData.Current.LocalSettings.Values["SuspendedTitle"];
                    MainPage.GoToArticle(args[0], title, args[1], null, null);
                }
                else
                    MainPage.GoToNews();
            }
            catch
            {
                MainPage.GoToNews();
            }
        }
    }
}