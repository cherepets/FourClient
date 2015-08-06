using FourAPI;
using FourAPI.Types;
using FourClient.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebServiceClient;
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
    public sealed partial class ArticleView : UserControl, IBackButton
    {
        public ArticleView()
        {
            this.InitializeComponent();
        }

        public string CurrentArticle { get; private set; }

        private bool _loaded;
        private WebView _webView;
        private string _fullLink;
        private string _commentLink;
        private string _html;

        public async void Load(string prefix, string link, string fullLink, string commentLink)
        {
            try
            {
                if (fullLink != null && commentLink != null)
                {
                    Share.Visibility = Visibility.Visible;
                    Globe.Visibility = Visibility.Visible;
                    Comment.Visibility = Visibility.Visible;
                    _fullLink = fullLink;
                    _commentLink = commentLink;
                }
                else
                {
                    Share.Visibility = Visibility.Collapsed;
                    Globe.Visibility = Visibility.Collapsed;
                    Comment.Visibility = Visibility.Collapsed;
                }
                CurrentArticle = prefix + ';' + link;
                _webView = new WebView();
                _webView.DefaultBackgroundColor = (this.Background as SolidColorBrush).Color;
                WebContent.Children.Clear();
                WebContent.Children.Add(_webView);
                var back = (this.Background as SolidColorBrush).Color.ToRGBString();
                var front = (this.Foreground as SolidColorBrush).Color.ToRGBString();
                var emptyView = String.Format("<html><body bgcolor='{0}' /></html>", back);
                _webView.NavigateToString(emptyView);
                var appData = ApplicationData.Current.LocalFolder;
                var tempData = ApplicationData.Current.TemporaryFolder;
                var article = await CheckCache(appData) ?
                    await LoadFromCache(appData) :
                    await CheckCache(tempData) ?
                        await LoadFromCache(tempData) : 
                        await LoadFromService(prefix, link);
                var html = article.HTML
                    .Replace("{0}", back)
                    .Replace("{1}", front)
                    .Replace("{2}", SettingsService.FontSize.ToString())
                    .Replace("{3}", SettingsService.FontFace)
                    .Replace("{4}", SettingsService.YouTube)
                    .Replace("{5}", SettingsService.Align);
                _loaded = false;
                _html = html;
                if (_webView == null) return;
                _webView.NavigateToString(_html);
                if (_webView == null) return;
                _webView.NavigationCompleted += webView_NavigationCompleted;
                if (_webView == null) return;
                while (!_loaded) await Task.Delay(100);
                if (_webView == null) return;
                _webView.NavigationCompleted -= webView_NavigationCompleted;
            }
            catch (ServiceException se)
            {
                var dialog = new MessageDialog(se.Message, "WebServiceClient.ServiceException");
                await dialog.ShowAsync();
                BackPressed();
            }
            catch (ConnectionException se)
            {
                var dialog = new MessageDialog(se.Message, "ConnectionException");
                await dialog.ShowAsync();
                BackPressed();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "FourClient.WebViewException");
                await dialog.ShowAsync();
                BackPressed();
            }
        }

        private async Task<FourArticle> LoadFromService(string prefix, string link)
        {
            MainPage.StatusProgress(true);
            var article = await Methods.GetArticleAsync(prefix, link);
            await SaveToTemp(article);
            MainPage.StatusProgress(false);
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

        private async Task<FourArticle> LoadFromCache(StorageFolder folder)
        {
            try
            {
                var files = await folder.GetFilesAsync();
                var filename = CurrentArticle.GetHashCode().ToString() + ".html";
                using (var stream = await files.First(f => f.Name == filename).OpenStreamForReadAsync())
                {
                    var xml = XDocument.Load(stream);
                    var article = new FourArticle
                    {
                        HTML = xml.Element("HTML").Value
                    };
                    return article;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(
@"Проблема при попытке загрузить данные из кэша.

Details:
{0}
{1}", ex.Message, ex.StackTrace));
            }
        }

        private async Task SaveToTemp(FourArticle article)
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
        
        private void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (_webView != null) _loaded = true;
        }
        
        public void BackPressed()
        {
            if (_webView == null) return;
            if (_webView.CanGoBack)
            {
                _webView = new WebView();
                _webView.DefaultBackgroundColor = (this.Background as SolidColorBrush).Color;
                WebContent.Children.Clear();
                WebContent.Children.Add(_webView);
                _webView.NavigateToString(_html);
                return;
            }
            WebContent.Children.Clear();
            _webView = null;
            MainPage.GoToNews();
        }
        
        private async void Globe_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri(_fullLink);
            await Launcher.LaunchUriAsync(uri);
        }

        private async void Comments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri(_commentLink);
            await Launcher.LaunchUriAsync(uri);
        }
        
        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareDataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = MainPage.GetTitle();
            request.Data.Properties.Description = "Отправлено из FourClient для Windows";
            try
            {
                var uri = new Uri(_fullLink);
                request.Data.SetWebLink(uri);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Delta.Translation.X > 10) BackPressed();
        }
    }
}
