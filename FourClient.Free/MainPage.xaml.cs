using FourClient.Extensions;
using FourClient.UserControls;
using FourClient.Views;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FourClient
{
    public sealed partial class MainPage : Page
    {
        public static string CurrentView { get { return CurrentPage.GetType().Name; } }
        public bool Alive { get; private set; }

        private static MainPage Singleton;
        private static IBackButton CurrentPage;
        private static string StatusText;

        public PageHeaderBase PageHeader;

        public MainPage()
        {
            FourAPI.Settings.Url = "http://cherryWebServiceClient.azurewebsites.net/TransformationService.asmx";
            this.InitializeComponent();
            Singleton = this;
            RebuildUI();
            GoToNews();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            RequestedTheme = SettingsService.MainTheme;
            ArticleView.RequestedTheme = SettingsService.ArticleTheme;
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void RebuildUI()
        {
            PageHeader = new StatusBar();
            PageHeaderGrid.Children.Clear();
            PageHeaderGrid.Children.Add(PageHeader);
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            CurrentPage.BackPressed();
            e.Handled = true;
        }

        public static async Task GoToArticle(string prefix, string name, string link, string fullLink, string commentLink)
        {
            StatusText = Singleton.PageHeader.InitialText;
            SetTitle(name);
            CurrentPage = Singleton.ArticleView;
            //    await Singleton.AnimateFromNews.PlayAsync();
            //    await Singleton.AnimateToArticle.PlayAsync();
            Singleton.ArticleView.Load(prefix, link, fullLink, commentLink);
            //    Singleton.NewsFeed.Visibility = Visibility.Collapsed;
            Singleton.UpdateVisualState();
        }

        public static void GoToAbout()
        {
            Singleton.Frame.Navigate(typeof(AboutPage));
        }

        public static void GoToSettings()
        {
            Singleton.Frame.Navigate(typeof(SettingsPage));
        }

        public static async Task GoToNews()
        {
            var text = !String.IsNullOrEmpty(StatusText) ? StatusText : "FourClient";
            //     Singleton.NewsFeed.Visibility = Visibility.Visible;
            SetTitle(text);
            CurrentPage = Singleton.NewsFeed;
            //     await Singleton.AnimateToNews.PlayAsync();
            Singleton.UpdateVisualState();
        }

        public static async Task GoToNewsFeed(string prefix)
        {
            await GoToNews();
            Singleton.NewsFeed.GoToSource(prefix);
        }

        public static Point GetSize()
        {
            return new Point(Singleton.ActualWidth, Singleton.ActualHeight);
        }

        public static void StatusProgress(bool val)
        {
            Singleton.PageHeader.IsProgressRunning = val;
        }

        public static void SetTitle(string title)
        {
            Singleton.PageHeader.SetTitle(title);
        }

        public static void SetTitle(string title, params object[] args)
        {
            var formated = String.Format(title, args);
            SetTitle(formated);
        }

        public static void SaveState()
        {
            ApplicationData.Current.LocalSettings.Values["SuspendedPage"] = MainPage.CurrentView;
            if (MainPage.CurrentView == "ArticleView")
            {
                ApplicationData.Current.LocalSettings.Values["SuspendedArticle"] = Singleton.ArticleView.CurrentArticle;
                ApplicationData.Current.LocalSettings.Values["SuspendedTitle"] = Singleton.PageHeader.InitialText;
            }
            else
                ApplicationData.Current.LocalSettings.Values["SuspendedArticle"] = null;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            var state = ActualWidth > ActualHeight ?
                "TwoPanes" :
                MainPage.CurrentView == "ArticleView" ?
                "RightPane" :
                "LeftPane";
            VisualStateManager.GoToState(this, state, false);
        }
    }
}
