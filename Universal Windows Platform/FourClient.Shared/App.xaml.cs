using FourClient.Background;
using FourClient.Data;
using FourClient.Library.Cache;
using FourClient.Library.Statistics;
using FourToolkit.Extensions.Runtime;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FourClient
{
    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Current.UnhandledException += Current_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        public static new App Current => Application.Current as App;

        public static void HandleException(Exception e) => ShowExceptionMessage(e);

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            await CacheBase.InitAsync();
            await StatisticsBase.InitAsync();
            if (Platform.IsMobile)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
            ChangeNameBarColor();
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
                //TODO: Implement launch from secondary tile
                //var argString = e.Arguments;
                //if (!string.IsNullOrEmpty(argString))
                //{
                //    var tiles = await SecondaryTile.FindAllForPackageAsync();
                //    var tile = tiles.FirstOrDefault(t => t.TileId == e.TileId);
                //    var title = tile == null ? "FourClient" : tile.DisplayName;
                //    var args = argString.Split(';').ToList();
                //    while (args.Count() > 2)
                //    {
                //        args[1] += ';' + args[2];
                //        args.Remove(args[2]);
                //    }
                //    switch (args.Count())
                //    {
                //        case 1:
                //            MainPage.GoToNewsFeed(args[0]);
                //            break;
                //        case 2:
                //            MainPage.GoToArticle(args[0], title, args[1], null, null, null);
                //            break;
                //    }
                //}
            }
            finally
            {
                Window.Current.Activate();
            }
            new NotifierBackgroundTask().Register(new TimeTrigger(60, false));
        }

        private static void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (e.Message == "Unspecified error\r\n" || e.Exception?.Message == null) return;
            HandleException(e.Exception);
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            if (e.Exception?.Message == null) return;
            HandleException(e.Exception);
        }

        private static async void ShowExceptionMessage(Exception e)
        {
            var dispatcher = Window.Current?.CoreWindow?.Dispatcher;
            if (dispatcher == null) return;
            await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                var wse = e as WebServiceException;
                var dialog = new MessageDialog(
                    wse == null ? e.Message : wse.Message,
                    e.GetType().Name)
                    .WithCommand("Close")
                    .SetCancelCommandIndex(0)
                    .SetDefaultCommandIndex(0);
                if (e.StackTrace != null)
                    dialog.WithCommand("StackTrace", () => ShowStackTrace(e));
                if (e.InnerException != null)
                    dialog.WithCommand("InnerException", () => ShowExceptionMessage(e.InnerException));
                await dialog.ShowAsync();
            });
        }

        private static async void ShowStackTrace(Exception e)
        {
            var dialog = new MessageDialog(e.StackTrace, e.GetType().Name)
                .WithCommand("Close")
                .SetCancelCommandIndex(0)
                .SetDefaultCommandIndex(0);
            if (e.Message != null)
                dialog.WithCommand("Message", () => ShowExceptionMessage(e));
            if (e.InnerException != null)
                dialog.WithCommand("InnerException", () => ShowExceptionMessage(e.InnerException));
            await dialog.ShowAsync();
        }

        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void ChangeNameBarColor()
        {
            if (!Platform.IsDesktop) return;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var accentBrush = Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
            if (accentBrush != null)
            {
                var accent = accentBrush.Color;
                var light = accent.Lighten();
                var dark = accent.Darken();
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
            }
            titleBar.InactiveForegroundColor = Colors.White;
        }

        private static void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            CacheBase.Close();
            StatisticsBase.Close();
            deferral.Complete();
        }
    }
}