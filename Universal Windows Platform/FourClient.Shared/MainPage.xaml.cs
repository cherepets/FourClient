using FourClient.UserControls;
using FourClient.Views;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

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
            InitializeComponent();
            Singleton = this;
            RebuildUI();
            GoToNews();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            RequestedTheme = SettingsService.MainTheme;
            ArticleView.RequestedTheme = SettingsService.ArticleTheme;
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }
        
        private void RebuildUI()
        {
            if (SettingsService.IsPhone)
            {
                PageHeader = new MobileHeader();
                PageHeaderGrid.Children.Clear();
                PageHeaderGrid.Children.Add(PageHeader);
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
        }

        public static void RebuildNewsFeedUI() => Singleton.NewsFeed?.RebuildUI();

        public static void GoToArticle(string prefix, string name, string link, string fullLink, string commentLink, string title)
        {
            _articleOpened = true;
            StatusText = GetTitle();
            SetTitle(name);
            var view = Singleton.ArticleView.Children[1] as ArticleView;
            CurrentPage = view;
            view.Load(prefix, link, fullLink, commentLink, title);
            Singleton.UpdateVisualState(true);
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
            var text = !string.IsNullOrEmpty(StatusText) ? StatusText : "FourClient";
            SetTitle(text);
            CurrentPage = Singleton.NewsFeed;
            Singleton.UpdateVisualState(true);
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
            if (Singleton.PageHeader != null)
                Singleton.PageHeader.IsProgressRunning = val;
        }

        public static string GetTitle()
        {
            if (SettingsService.IsPhone)
                return Singleton.PageHeader?.InitialText;
            else
                return Singleton.HeaderBlock.Text;
        }

        public static void SetTitle(string title)
        {
            if (SettingsService.IsPhone)
                Singleton.PageHeader?.SetTitle(title);
            else
                Singleton.HeaderBlock.Text = _articleOpened ? title : string.Empty;
        }

        public static void SetTitle(string title, params object[] args)
        {
            var formated = string.Format(title, args);
            SetTitle(formated);
        }

        public static void SaveState()
        {
            if (_articleOpened)
            {
                var view = Singleton.ArticleView.Children[1] as ArticleView;
                ApplicationData.Current.LocalSettings.Values["SuspendedArticle"] = view.CurrentArticle;
                ApplicationData.Current.LocalSettings.Values["SuspendedTitle"] = GetTitle();
            }
            else
                ApplicationData.Current.LocalSettings.Values["SuspendedArticle"] = null;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState(bool skipBindingsInvalidation = false)
        {
            var state = ActualWidth > ActualHeight ?
                "TwoPanes" :
                _articleOpened ?
                "RightPane" :
                "LeftPane";
            VisualStateManager.GoToState(this, state, false);
            if (!skipBindingsInvalidation && (state != "TwoPanes" || PrevVisualState != "TwoPanes")) 
            {
                NewsFeed.RebuildUI();
            }
            BackButton.Visibility = !SettingsService.IsPhone && state == "RightPane"
                ? Visibility.Visible 
                : Visibility.Collapsed;
            HeaderBlock.Visibility = !SettingsService.IsPhone && (state == "RightPane" || state == "TwoPanes")
                ? Visibility.Visible
                : Visibility.Collapsed;
            PrevVisualState = state;
        }
        
        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            CurrentPage.BackPressed();
            e.Handled = true;
        }

        private void BackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CurrentPage.BackPressed();
            e.Handled = true;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            CurrentPage.BackPressed();
            e.Handled = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= MainPage_BackRequested;
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }
    }
}
