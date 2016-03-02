using FourClient.Data;
using FourClient.Library;
using FourClient.Library.Cache;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace FourClient.Views
{
    public delegate void StateEventHandler(bool newState);

    public interface IArticleView
    {
        bool Opened { get; }
        void Open(Article article);
        void Close();
        event StateEventHandler StateChanged;
    }

    public sealed partial class ArticleView : IArticleView
    {
        public delegate void ViewEventHandler(IArticleView sender);
        public static event ViewEventHandler ViewLoaded;

        public event StateEventHandler StateChanged;

        public bool Opened
        {
            get
            {
                return _opened;
            }
            private set
            {
                _opened = value;
                StateChanged?.Invoke(_opened);
            }
        }
        private bool _opened;

        public Article Article { get; private set; }

        public ArticleView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
            _uiUpdateTimer.Tick += (s, a) => UpdateUi();
        }

        private DispatcherTimer _uiUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };

        private const int UiTimeout = 30;

        private bool _loaded;
        private HtmlRender _render;
        private int hideUiAfter;
        
        private void UpdateUi()
        {
            if (!UiIsVisible) return;
            if (hideUiAfter > 0)
                hideUiAfter--;
            if (hideUiAfter == 0) HideUi();
        }

        public async void Open(Article article)
        {
            Opened = true;
            if (string.IsNullOrEmpty(article.Title))
                article.FillFromCache(Data.IoC.TopCache as TopCache);
            StatusBar.Visibility = Visibility.Collapsed;
            if (_uiUpdateTimer.IsEnabled) _uiUpdateTimer.Stop();
            HideUi();
            ProgressRing.IsActive = true;
            WebContent.Visibility = Visibility.Collapsed;
            Article = article;
            UpdateStarState();
            var back = string.Empty;
            var front = string.Empty;
            try
            {
                TitleBlock.Text = Article?.Title;
                _render = new HtmlRender();
                _render.ScrollUp += (s, a) => ShowUi();
                _render.ScrollDown += (s, a) => HideUi();
                _render.Background = (Background as SolidColorBrush).Color;
                _render.Foreground = (Foreground as SolidColorBrush).Color;
                _render.FontSize = Settings.Current.FontSize;
                WebContent.Children.Clear();
                WebContent.Children.Add(_render.Implementation);
                back = (Background as SolidColorBrush).Color.ToRgbString();
                front = (Foreground as SolidColorBrush).Color.ToRgbString();
                var emptyView = string.Format("<html><body bgcolor='{0}' /></html>", back);
                _render.Html = emptyView;
                if (string.IsNullOrEmpty(Article?.Html))
                    LoadFromCache();
                if (string.IsNullOrEmpty(Article?.Html))
                    await LoadFromService();
                if (string.IsNullOrEmpty(Article?.Html))
                {
                    Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                App.HandleException(ex);
                Close();
                return;
            }
            var html = Article.Html
                    .Replace("{0}", back)
                    .Replace("{1}", front)
                    .Replace("{2}", Settings.Current.FontSize.ToString())
                    .Replace("{3}", Settings.Current.FontFace)
                    .Replace("{4}", Settings.Current.YouTube)
                    .Replace("{5}", Settings.Current.Align)
                    .Replace("{6}", Settings.Current.ScrollEventThreshold.ToString());
            _loaded = false;
            if (_render != null)
                lock (_render)
                {
                    _render.Completed += render_Completed;
                    _render.Html = html;
                }
        }

        private void BottomFiller_PointerMoved(object sender, PointerRoutedEventArgs e) => ShowUi();
        private void BottomFiller_Tapped(object sender, TappedRoutedEventArgs e) => ShowUi();

        private bool UiIsVisible
            => AppBar.VisibleHeight > 0
            && AppBar.Visibility == Visibility.Visible
            && TitleBlock.Visibility == Visibility.Visible;

        private async void ShowUi()
        {
            hideUiAfter = UiTimeout;
            if (!_loaded) return;
            if (UiIsVisible) return;
            AppBar.Visibility = Visibility.Visible;
            TitleBlock.Visibility = Visibility.Visible;
            await AppBar.ShowAsync();
            AppBar.Visibility = Visibility.Visible;
        }

        private async void HideUi()
        {
            if (!_loaded) return;
            if (!UiIsVisible) return;
            TitleBlock.Visibility = Visibility.Collapsed;
            await AppBar.HideAsync();
            AppBar.Visibility = Visibility.Collapsed;
        }

        public void Close()
        {
            if (_uiUpdateTimer.IsEnabled) _uiUpdateTimer.Stop();
            Opened = false;
            StatusBar.Visibility = Visibility.Visible;
            Article = null;
            WebContent.Children.Clear();
            _render = null;
            if (Platform.IsMobile)
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
        }

        private void LoadFromCache()
            => Article = Article == null ? null
            : IoC.ArticleCache.FindInCache(Article.Prefix, Article.Link) ?? Article;

        private async Task LoadFromService()
        {
            try
            {
                if (Article == null) return;
                Article.InCollection = false;
                Article.Html = await Api.GetArticleAsync(Article.Prefix, Article.Link);
            }
            catch (WebServiceException ex)
            {
                App.HandleException(ex);
                return;
            }
            catch (ConnectionException ex)
            {
                App.HandleException(ex);
                return;
            }
            SaveToTemp();
        }

        private void SaveToTemp()
        {
            if (!string.IsNullOrEmpty(Article?.Html))
            {
                Article.CreatedOn = DateTime.Now;
                IoC.ArticleCache.Put(Article);
            }
        }
        
        private void render_Completed(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(Article.Html))
                return;
            IoC.KeywordStatistics.UpdateWith(Article.Title);
            WebContent.Visibility = Visibility.Visible;
            ProgressRing.IsActive = false;
            _loaded = true;
            UpdateStarState();
            ShowUi();
            if (!_uiUpdateTimer.IsEnabled) _uiUpdateTimer.Start();
            if (Platform.IsMobile && Settings.Current.AllowRotation)
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped;
            if (_render != null)
                _render.Completed -= render_Completed;
        }

        private async void Star_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hideUiAfter = UiTimeout;
            if (Article == null) return;
            Article.InCollection = !Article.InCollection;
            var result = IoC.ArticleCache.UpdateCollectionState(Article);
            if (!result)
            {
                var dialog = new MessageDialog("Ошибка при работе с коллекцией!");
                await dialog.ShowAsync();
            }
            UpdateStarState();
        }

        private async void Globe_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hideUiAfter = UiTimeout;
            if (Article == null) return;
            var uri = new Uri(Article.FullLink);
            await Launcher.LaunchUriAsync(uri);
        }

        private async void Comments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hideUiAfter = UiTimeout;
            if (Article == null) return;
            var uri = new Uri(Article.CommentLink);
            await Launcher.LaunchUriAsync(uri);
        }
        
        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hideUiAfter = UiTimeout;
            if (Article == null) return;
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareDataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = Article.Title;
            request.Data.Properties.Description = "Отправлено из FourClient для Windows 10";
            try
            {
                var uri = new Uri(Article.FullLink);
                request.Data.SetWebLink(uri);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void UpdateStarState()
            => StarButton.Icon = Article.InCollection ? FourToolkit.UI.Icon.StarFilled : FourToolkit.UI.Icon.Star;

        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Delta.Translation.X > 5) Close();
        }
    }
}
