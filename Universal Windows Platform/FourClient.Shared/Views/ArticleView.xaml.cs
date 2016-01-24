using FourClient.Data;
using FourToolkit.UI.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
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
        void Open(string prefix, string link, string fullLink, string commentLink, string title);
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

        public ArticleView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
        }

        public string CurrentArticle { get; private set; }

        private bool _loaded;
        private HtmlRender _render;
        private string _prefix;
        private string _link;
        private string _fullLink;
        private string _commentLink;
        private string _title;
        private string _html;
        
        public async void Open(string prefix, string link, string fullLink, string commentLink, string title)
        {
            Opened = true;
            StatusBar.Visibility = Visibility.Collapsed;
            HideUi();
            ProgressRing.IsActive = true;
            WebContent.Visibility = Visibility.Collapsed;
            Article article;
            var back = string.Empty;
            var front = string.Empty;
            try
            {
                _prefix = prefix;
                _link = link;
                _fullLink = fullLink;
                _commentLink = commentLink;
                TitleBlock.Text = _title = title;
                CurrentArticle = prefix + ';' + link;
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
                var appData = ApplicationData.Current.LocalFolder;
                var tempData = ApplicationData.Current.TemporaryFolder;
                article = await CheckCache(appData) ?
                    await LoadFromCache(appData) :
                    await CheckCache(tempData) ?
                        await LoadFromCache(tempData) :
                        await LoadFromService(prefix, link);
            }
            catch (WebServiceException se)
            {
                var dialog = new MessageDialog(se.Message, "WebServiceClient.ServiceException");
                await dialog.ShowAsync();
                Close();
            }
            catch (ConnectionException se)
            {
                var dialog = new MessageDialog(se.Message, "ConnectionException");
                await dialog.ShowAsync();
                Close();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "FourClient.WebViewException");
                await dialog.ShowAsync();
                Close();
            }
            if (string.IsNullOrEmpty(article.HTML)) return;
            try
            {
                var html = article.HTML
                        .Replace("{0}", back)
                        .Replace("{1}", front)
                        .Replace("{2}", Settings.Current.FontSize.ToString())
                        .Replace("{3}", Settings.Current.FontFace)
                        .Replace("{4}", Settings.Current.YouTube)
                        .Replace("{5}", Settings.Current.Align);
                _loaded = false;
                _html = html;
                if (_render == null) return;
                _render.Completed += render_Completed;
                if (_render == null) return;
                _render.Html = _html;
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
            CurrentArticle = null;
            WebContent.Children.Clear();
            _render = null;
            //       MainPage.GoToNews();
        }

        private async Task<Article> LoadFromService(string prefix, string link)
        {
            var article = await Api.GetArticleAsync(prefix, link);
            await SaveToTemp(article);
            return article;
        }

        private async Task<bool> CheckCache(StorageFolder folder)
        {
            try
            {
                var files = await folder.GetFilesAsync();
                var filename = CurrentArticle.GetHashCode().ToString() + ".html";
                return files.Any(f => f.Name == filename);
            }
            catch
            {
                return false;
            }
        }

        private async Task<Article> LoadFromCache(StorageFolder folder)
        {
            try
            {
                var files = await folder.GetFilesAsync();
                var filename = CurrentArticle.GetHashCode().ToString() + ".html";
                using (var stream = await files.First(f => f.Name == filename).OpenStreamForReadAsync())
                {
                    var xml = XDocument.Load(stream);
                    var article = new Article
                    {
                        HTML = xml.Element("HTML").Value
                    };
                    return article;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(
@"Проблема при попытке загрузить данные из кэша.

Details:
{0}
{1}", ex.Message, ex.StackTrace));
            }
        }

        private async Task SaveToTemp(Article article)
        {
            try
            {
                var xml = new XDocument();
                xml.Add(new XElement("HTML", article.HTML));
                var appData = ApplicationData.Current.TemporaryFolder;
                var filename = CurrentArticle.GetHashCode().ToString() + ".html";
                var file = await appData.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var bytes = Encoding.UTF8.GetBytes(xml.ToString());
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            catch { }
        }
        
        private void render_Completed(object sender, EventArgs args)
        {
            ShowUi();
            WebContent.Visibility = Visibility.Visible;
            ProgressRing.IsActive = false;
            if (_render != null) _loaded = true;
        }
                
        private async void Globe_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentArticle == null) return;
            var uri = new Uri(_fullLink);
            await Launcher.LaunchUriAsync(uri);
        }

        private async void Comments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentArticle == null) return;
            var uri = new Uri(_commentLink);
            await Launcher.LaunchUriAsync(uri);
        }
        
        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentArticle == null) return;
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareDataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = _title;
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
