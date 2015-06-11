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
        public bool Alive { get; private set; }

        private static MainPage Singleton;
        private static IBackButton CurrentPage;
        private static string StatusText;
        private static string PrevVisualState;

        private static bool _articleOpened;

        public PageHeaderBase PageHeader;

        public MainPage()
        {
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

            DisplayInformation.AutoRotationPreferences = SettingsService.IsPhablet ?
                DisplayOrientations.LandscapeFlipped | DisplayOrientations.Portrait | DisplayOrientations.Landscape :
                DisplayOrientations.Portrait;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            CurrentPage.BackPressed();
            e.Handled = true;
        }

        public static void GoToArticle(string prefix, string name, string link, string fullLink, string commentLink)
        {
            _articleOpened = true;
            StatusText = Singleton.PageHeader.InitialText;
            SetTitle(name);
            CurrentPage = Singleton.ArticleView;
            Singleton.ArticleView.Load(prefix, link, fullLink, commentLink);
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

        public static void GoToNews()
        {
            _articleOpened = false;
            var text = !String.IsNullOrEmpty(StatusText) ? StatusText : "FourClient";
            SetTitle(text);
            CurrentPage = Singleton.NewsFeed;
            Singleton.UpdateVisualState();
        }

        public static void GoToNewsFeed(string prefix)
        {
            GoToNews();
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
            if (_articleOpened)
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
                _articleOpened ?
                "RightPane" :
                "LeftPane";
            if (state == PrevVisualState) return;
            VisualStateManager.GoToState(this, state, false);
            if ((state == "TwoPanes" && PrevVisualState != "TwoPanes") 
                || (state != "TwoPanes" && PrevVisualState == "TwoPanes"))
            {
                NewsFeed.InvalidateBindings();
            }
            PrevVisualState = state;
        }
    }
}
