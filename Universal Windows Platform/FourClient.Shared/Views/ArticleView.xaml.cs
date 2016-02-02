using FourClient.Data;
using FourToolkit.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
            if (hideUiAfter > 0)
                hideUiAfter--;
            if (hideUiAfter == 0) HideUi();
        }

        public async void Open(Article article)
        {
            Opened = true;
            StatusBar.Visibility = Visibility.Collapsed;
            HideUi();
            ProgressRing.IsActive = true;
            WebContent.Visibility = Visibility.Collapsed;
            Article = article;
            var back = string.Empty;
            var front = string.Empty;
            try
            {
                TitleBlock.Text = Article.Title;
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
                if (string.IsNullOrEmpty(Article.Html))
                    LoadFromCache();
                if (string.IsNullOrEmpty(Article.Html))
                    await LoadFromService();
                if (string.IsNullOrEmpty(Article.Html))
                    throw new ArgumentNullException("Article is null at unexpected time");
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "FourClient.ArticleView");
                await dialog.ShowAsync();
                Close();
            }
            try
            {
                var html = Article.Html
                        .Replace("{0}", back)
                        .Replace("{1}", front)
                        .Replace("{2}", Settings.Current.FontSize.ToString())
                        .Replace("{3}", Settings.Current.FontFace)
                        .Replace("{4}", Settings.Current.YouTube)
                        .Replace("{5}", Settings.Current.Align);
                _loaded = false;
                if (_render == null) return;
                _render.Completed += render_Completed;
                if (_render == null) return;
                _render.Html = html;
                if (_render == null) return;
                while (!_loaded) await Task.Delay(100);
            }
            finally
            {
                if (_render != null)
                    _render.Completed -= render_Completed;
            }
        }

        private async void ShowUi()
        {
            if (AppBar.Visibility == Visibility.Visible) return;
            AppBar.Visibility = Visibility.Visible;
            TitleBlock.Visibility = Visibility.Visible;
            hideUiAfter = UiTimeout;
            await AppBar.ShowAsync();
        }

        private async void HideUi()
        {
            if (AppBar.Visibility == Visibility.Collapsed) return;
            TitleBlock.Visibility = Visibility.Collapsed;
            await AppBar.HideAsync();
            AppBar.Visibility = Visibility.Collapsed;
        }

        public void Close()
        {
            if (_uiUpdateTimer.IsEnabled) _uiUpdateTimer.Stop();
            Opened = false;
            StatusBar.Visibility = Visibility.Visible;
            //if (_render.CanGoBack)
            //{
            //    _render = new EdgeHtml();
            //    _render.Background = (Background as SolidColorBrush).Color;
            //    WebContent.Children.Clear();
            //    WebContent.Children.Add(_render.Implementation);
            //    _render.Html = _html;
            //    return;
            //}
            Article = null;
            WebContent.Children.Clear();
            _render = null;
        }

        private void LoadFromCache() 
            => Article.Html = IoC.ArticleCache.FindHtml(Article.Prefix, Article.Link);

        private async Task LoadFromService()
        {
            try
            {
                Article.Html = await Api.GetArticleAsync(Article.Prefix, Article.Link);
            }
            catch (WebServiceException se)
            {
                var dialog = new MessageDialog(se.Message, "WebServiceException");
                await dialog.ShowAsync();
                Close();
            }
            catch (ConnectionException se)
            {
                var dialog = new MessageDialog(se.Message, "ConnectionException");
                await dialog.ShowAsync();
                Close();
            }
            SaveToTemp();
        }

        private void SaveToTemp()
        {
            if (!string.IsNullOrEmpty(Article.Html))
            {
                Article.CreatedOn = DateTime.Now;
                IoC.ArticleCache.Put(Article);
            }
        }
        
        private void render_Completed(object sender, EventArgs args)
        {
            ShowUi();
            WebContent.Visibility = Visibility.Visible;
            ProgressRing.IsActive = false;
            if (_render != null) _loaded = true;
            if (!_uiUpdateTimer.IsEnabled) _uiUpdateTimer.Start();
        }
                
        private async void Globe_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Article == null) return;
            var uri = new Uri(Article.FullLink);
            await Launcher.LaunchUriAsync(uri);
        }

        private async void Comments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Article == null) return;
            var uri = new Uri(Article.CommentLink);
            await Launcher.LaunchUriAsync(uri);
        }
        
        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
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
                //var uri = new Uri(string.Format(Settings.Current.ShareTemplate, _prefix, _link, _title));
                //request.Data.SetWebLink(uri);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Delta.Translation.X > 5) Close();
        }
    }
}
