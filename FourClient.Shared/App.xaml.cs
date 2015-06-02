using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            await Windows.UI.ViewManagement
                .StatusBar
                .GetForCurrentView()
                .HideAsync();
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
                if (!String.IsNullOrEmpty(argString))
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
                            await MainPage.GoToNewsFeed(args[0]);
                            break;
                        case 2:
                            await MainPage.GoToArticle(args[0], title, args[1], null, null);
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

        private async void OnResuming(object sender, object e)
        {
            try
            {
                if (MainPage.CurrentView != "NewsFeed") return;
                var pageName = (string)ApplicationData.Current.LocalSettings.Values["SuspendedPage"];
                switch (pageName)
                {
                    case "AboutView":
                        MainPage.GoToAbout();
                        break;
                    case "SettingsView":
                        MainPage.GoToSettings();
                        break;
                    case "ArticleView":
                        var article = (string)ApplicationData.Current.LocalSettings.Values["SuspendedArticle"];
                        var args = article.Split(';');
                        var title = (string)ApplicationData.Current.LocalSettings.Values["SuspendedTitle"];
                        await MainPage.GoToArticle(args[0], title, args[1], null, null);
                        break;
                    case "NewsFeed":
                        await MainPage.GoToNews();
                        break;
                }
            }
            catch
            {
                MainPage.GoToNews();
            }
        }
    }
}