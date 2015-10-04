using FourApi;
using FourApi.Types;
using FourClient.Extensions;
using FourClient.HtmlRender;
using FourClient.UserControls;
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
            InitializeComponent();
        }

        public string CurrentArticle { get; private set; }

        private bool _loaded;
        private IHtmlRender _render;
        private string _prefix;
        private string _link;
        private string _fullLink;
        private string _commentLink;
        private string _title;
        private string _html;

        public async void Load(string prefix, string link, string fullLink, string commentLink, string title)
        {
            ProgressRing.IsActive = true;
            Render.Visibility = Visibility.Collapsed;
            try
            {
                if (fullLink != null && commentLink != null)
                {
                    Share.Visibility = Visibility.Visible;
                    Globe.Visibility = Visibility.Visible;
                    Comment.Visibility = Visibility.Visible;
                    _prefix = prefix;
                    _link = link;
                    _fullLink = fullLink;
                    _commentLink = commentLink;
                    _title = title;
                }
                else
                {
                    Share.Visibility = Visibility.Collapsed;
                    Globe.Visibility = Visibility.Collapsed;
                    Comment.Visibility = Visibility.Collapsed;
                }
                CurrentArticle = prefix + ';' + link;
                _render = HtmlRenderFactory.GetRender();
                _render.Background = (Background as SolidColorBrush).Color;
                _render.Foreground = (Foreground as SolidColorBrush).Color;
                _render.FontSize = SettingsService.FontSize;
                WebContent.Children.Clear();
                WebContent.Children.Add(_render.Implementation);
                var back = (Background as SolidColorBrush).Color.ToRGBString();
                var front = (Foreground as SolidColorBrush).Color.ToRGBString();
                var emptyView = string.Format("<html><body bgcolor='{0}' /></html>", back);
                _render.Html = emptyView;
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
                if (SettingsService.RenderSwitch && fullLink != null && commentLink != null)
                    Render.Visibility = Visibility.Visible;
                if (_render == null) return;
                _render.Completed += render_Completed;
                if (_render == null) return;
                _render.Html = _html;
                if (_render == null) return;
                while (!_loaded) await Task.Delay(100);
                if (_render == null) return;
                _render.Completed -= render_Completed;
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
                throw new Exception(string.Format(
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
        
        private void render_Completed(object sender, EventArgs args)
        {
            ProgressRing.IsActive = false;
            if (_render != null) _loaded = true;
        }
        
        public void BackPressed()
        {
            if (_render == null) return;
            if (_render.CanGoBack)
            {
                _render = new EdgeHtml();
                _render.Background = (Background as SolidColorBrush).Color;
                WebContent.Children.Clear();
                WebContent.Children.Add(_render.Implementation);
                _render.Html = _html;
                return;
            }
            WebContent.Children.Clear();
            _render = null;
            MainPage.GoToNews();
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
            request.Data.Properties.Title = MainPage.GetTitle();
            request.Data.Properties.Description = "Отправлено из FourClient для Windows 10";
            try
            {
                var uri = new Uri(string.Format(SettingsService.ShareTemplate, _prefix, _link, _title));
                request.Data.SetWebLink(uri);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void Rectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Delta.Translation.X > 6) BackPressed();
        }

        private void Render_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var list = new List<ListViewItem>();
            foreach (var renderName in HtmlRenderFactory.Renders)
            {
                var render = new ListViewItem() { Content = renderName, Tag = renderName };
                render.Tapped += render_Tapped;
                list.Add(render);
            }
            ShowHoverListView(list, Render);
        }

        private void render_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var render = (sender as ListViewItem).Tag as string;
            HideHoverListView();
            _render = HtmlRenderFactory.GetRender(render);
            _render.Background = (Background as SolidColorBrush).Color;
            _render.Foreground = (Foreground as SolidColorBrush).Color;
            _render.FontSize = SettingsService.FontSize;
            WebContent.Children.Clear();
            WebContent.Children.Add(_render.Implementation);
            _render.Html = _html;
        }

        private void ShowHoverListView(List<ListViewItem> items, FrameworkElement element)
        {
            if (HoverGrid.Visibility == Visibility.Visible) return;
            var top = element.GetPosition().Y;
            if (top < 0) top = 10;
            if (top > ActualHeight - 200) top = ActualHeight - 200;
            var hover = new HoverListView
            {
                HorizontalAlignment = SettingsService.IsPhone ? HorizontalAlignment.Stretch : HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, top, 0, 0)
            };
            items.ForEach(hover.Items.Add);
            HoverGrid.Children.Add(hover);
            HoverGrid.Visibility = Visibility.Visible;
            HoverGrid.Animate();
        }

        private void HideHoverListView()
        {
            HoverGrid.Children.Clear();
            HoverGrid.Visibility = Visibility.Collapsed;
        }

        private void HoverGrid_Tapped(object sender, TappedRoutedEventArgs e) => HideHoverListView();

        private void HoverGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) => HideHoverListView();
    }
}
