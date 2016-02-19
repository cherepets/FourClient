using FourClient.Cache;
using FourClient.Data;
using FourClient.Data.Feed;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FourClient
{
    public interface IMainPage
    {
        Flyout Flyout { get; }
        void ShowFlyout(UIElement element);
        void HideFlyout();
        CoreDispatcher Dispatcher { get; }
    }

    public sealed partial class MainPage : IMainPage
    {
        public delegate void PageEventHandler(IMainPage sender);
        public static event PageEventHandler PageLoaded;
                
        public MainPage()
        {
            IoC.RegisterViews();
            InitializeComponent();
            Loaded += MainPage_Loaded;
            PageLoaded?.Invoke(this);
            IoC.ArticleView.StateChanged += ArticleView_StateChanged;

            switch (Platform.Current)
            {
                case Platform.Desktop:
                    SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
                    {
                        if (!Flyout.IsOpen && _state != "RightPane")
                        {
                            if (MenuView.HandleBackButton())
                                e.Handled = true;
                        }
                    };
                    return;
                case Platform.Mobile:
                    HardwareButtons.BackPressed += (s, e) =>
                    {
                        if (!Flyout.IsOpen && _state != "RightPane")
                        {
                            if (MenuView.HandleBackButton())
                                e.Handled = true;
                        }
                    };
                    return;
            }
        }

        private string _state;
        private TimeSpan _animationLength = TimeSpan.FromSeconds(0.2);

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterDataDepedencies();
            Settings.Current.LastVersion = Settings.Current.CurrentVersion;
            Settings.Current.PropertyChanged += Settings_PropertyChanged;
            ApplySettings(Settings.Current);
            var top = Api.GetTop();
            IoC.InterestingView.SetItemsSource(top);
            if (Settings.Current.LiveTile) Notifier.UpdateMainTile(top);
            var sources = Api.GetSources();
            IoC.SourcesView.SetItemsSource(sources);
            var collection = new ObservableCollection<Article>();
            await Task.Run(() =>
                {
                    IoC.ArticleCache.RemoveOldEntites();
                    collection = IoC.ArticleCache.GetCollection();
                });
            IoC.CollectionView.SetItemsSource(collection);
            InitFirstPage(sources.FirstOrDefault());
        }

        private void InitFirstPage(Source defaultSource)
        {
            switch (Settings.Current.ShowAtStartup)
            {
                case StartUpType.Interesting:
                    IoC.MenuView.OpenInterestingTab();
                    break;
                case StartUpType.Sources:
                    IoC.MenuView.OpenSourcesTab();
                    break;
                case StartUpType.Feed:
                    var defaultFeed = new AbstractFeed
                    {
                        Source = defaultSource,
                        Topic = "?"
                    };
                    IoC.FeedView.SetItemsSource(defaultFeed);
                    IoC.MenuView.OpenFeedTab();
                    break;
                case StartUpType.Collection:
                    IoC.MenuView.OpenCollectionTab();
                    break;
            }
            this.AnimateFadeIn();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e) => ApplySettings(Settings.Current);

        private void ApplySettings(Settings e)
        {
            RequestedTheme = e.AppTheme ? ElementTheme.Light : ElementTheme.Dark;
            if (RequestedTheme == ElementTheme.Dark)
            {
                StatusBar.ForegroundBrush = new SolidColorBrush(Colors.White);
                StatusBar.BackgroundBrush = new SolidColorBrush(Colors.Black);
            }
            else
            {
                StatusBar.ForegroundBrush = new SolidColorBrush(Colors.Black);
                StatusBar.BackgroundBrush = new SolidColorBrush(Colors.White);
            }
        }

        private static void RegisterDataDepedencies()
        {
            var sourceCache = new SourceCache();
            var topicCache = new TopCache();
            Data.IoC.RegisterDependencies(IoC.SourcesView, sourceCache, topicCache);
        }

        private void ArticleView_StateChanged(bool newState) => UpdateVisualState(newState);

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateVisualState(IoC.ArticleView.Opened);

        private void UpdateVisualState(bool paneOpened)
        {
            _state = !Platform.IsMobile
                && ActualWidth > 800
                && ActualWidth > ActualHeight ?
                "TwoPanes" :
                paneOpened ?
                "RightPane" :
                "LeftPane";
            if (_state == "RightPane")
                this.AttachBackHandler(Back);
            else
                if (!Flyout.IsOpen)
                this.DetachBackHandler();
            VisualStateManager.GoToState(this, _state, false);
        }

        Flyout IMainPage.Flyout => Flyout;
        public void ShowFlyout(UIElement element)
        {
            this.AttachBackHandler(HideFlyout);
            var animate = Flyout.CurrentContent == null;
            Flyout.ShowFlyout(element);
            if (animate)
                element.AnimateFadeIn(_animationLength);
        }

        public async void HideFlyout()
        {
            this.DetachBackHandler();
            Flyout.CurrentContent.AnimateFadeOut(_animationLength);
            await Task.Delay(_animationLength);
            Flyout.HideFlyout();
        }

        private static void Back() => IoC.ArticleView.Close();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.DetachBackHandler();
        }
    }
}
