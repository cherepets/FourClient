using FourApi;
using FourApi.Types;
using FourClient.Extensions;
using FourClient.UserControls;
using NotificationsExtensions.TileContent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace FourClient.Views
{
    public sealed partial class NewsFeed : UserControl, IBackButton
    {
        private bool _sliding;
        private bool _flyoutOpened;
        private FourItem _dataContext;
        private FourItem _shareContext;
        private FourSource _sourceContext;
        private FourSource _source;
        private Task _loader;

        public PageCollection PageCollection { get; private set; }
        public ObservableCollection<FourItem> TopList { get; private set; }
        public ObservableCollection<FourItem> PageList { get; private set; }
        public ObservableCollection<FourItem> HistoryList { get; private set; }
        public ObservableCollection<FourSource> SourceList { get; private set; }
        public ObservableCollection<FourSource> HiddenList { get; private set; }

        private const string COLLECTION = "Collection198.xml";
        private const string SOURCES = "Sources220.xml";
        private const string HIDDEN = "Hidden220.xml";
        private const string TOP = "Top260.xml";
        private const string HISTORY = "History300";

        private List<Grid> _feedItems = new List<Grid>();
        private List<Grid> _topItems = new List<Grid>();
        
        private Dictionary<ScrollViewer, double> _oldScroll = new Dictionary<ScrollViewer, double>();

        public NewsFeed()
        {
            InitializeComponent();
            _loader = Load();
        }

        public void RebuildUI()
        {
            AfterLoad();

            PivotControl.Style = Application.Current.Resources["HeaderlessPivotStyle"] as Style;
            if (SettingsService.UpperMenu)
            {
                UpperHiddenBlock.Visibility = Visibility.Visible;
                LeftHiddenBlock.Visibility = Visibility.Collapsed;
                UpperView.Visibility = Visibility.Visible;
                LeftView.Visibility = Visibility.Collapsed;
                LeftPivotHeader.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpperHiddenBlock.Visibility = Visibility.Collapsed;
                LeftHiddenBlock.Visibility = Visibility.Visible;
                UpperView.Visibility = Visibility.Collapsed;
                LeftView.Visibility = Visibility.Visible;
                LeftPivotHeader.Visibility = Visibility.Visible;
            }

            CollectionView.ItemsSource = null;
            SourceView.ItemsSource = null;
            HiddenView.ItemsSource = null;
            CollectionView.ItemsSource = PageList;
            SourceView.ItemsSource = SourceList;
            HiddenView.ItemsSource = HiddenList;
        }

        #region Load
        private async Task Load()
        {
            await FullInit();
            var collectionTask = LoadCollection();
            var sourceTask = LoadSource();
            var topTask = LoadTop();
            var historyTask = LoadHistory();
            await collectionTask;
            await sourceTask;
            await topTask;
            await historyTask;
        }

        private async Task FullInit()
        {
            while (TopView.ActualWidth == 0)
            {
                await Task.Delay(1);
            }
        }

        private void LoadPage(string newsType = null)
        {
            if (_source == null) return;
            AppBarButtonRefresh.IsEnabled = true;
            AppBarButtonTopics.IsEnabled = _source.NewsTypes.Count > 1;
            SearchButton.IsEnabled = _source.Searchable;
            FeedRing.IsActive = true;
            if (newsType == null && _source != null)
                newsType = _source.NewsTypes.First().Key;
            var builder = new StringBuilder();
            foreach (var source in SourceList)
            {
                builder.Append(source.Prefix);
                builder.Append(";");
            }
            _source.MySources = builder.ToString();
            PageCollection = new PageCollection
            {
                NewsType = newsType,
                SearchMode = false,
                Source = _source
            };
            PageCollection.LoadFailed += PageList_LoadFailed;
            PageCollection.ServiceExceptionThrown += PageList_ServiceExceptionThrown;
            PageCollection.ConnectionExceptionThrown += PageCollection_ConnectionExceptionThrown;
            PageCollection.LoadStarted += () => MainPage.StatusProgress(true);
            PageCollection.LoadCompleted += () =>
            {
                FeedRing.IsActive = false;
                FeedCaption.Visibility = Visibility.Collapsed;
                MainPage.StatusProgress(false);
            };
            FeedView.ItemsSource = PageCollection;
        }

        private async Task LoadHistory()
        {
            var appData = ApplicationData.Current.LocalFolder;
            try
            {
                HistoryList = new ObservableCollection<FourItem>();
                var file = await appData.GetFileAsync(HISTORY);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var xdoc = XDocument.Load(stream);
                    foreach (var xsource in xdoc.Descendants("History"))
                    {
                        HistoryList.Add(
                            new FourItem
                            {
                                Link = xsource.Element("Link").Value,
                                Title = xsource.Element("Title").Value,
                            });
                    }
                }
            }
            catch { }
        }

        private async Task LoadTop(string newsType = null)
        {
            try
            {
                var task = Methods.GetTopAsync(TOP);
                await LoadCachedTop();
                if (task.Status != TaskStatus.RanToCompletion)
                    TopView.ItemsSource = TopList;
                // Renew cache
                try
                {
                    var newTop = await task;
                    // Checks lists for equality
                    bool equal = true;
                    if (newTop.Count == TopList.Count)
                    {
                        for (int i = 0; i < newTop.Count; i++)
                        {
                            if (newTop[i].FullLink != TopList[i].FullLink) equal = false;
                        }
                    }
                    else equal = false;
                    // Renew if not equal
                    if (!equal && newTop.Any())
                    {
                        TopList = newTop.ToObservable();
                        TopView.ItemsSource = TopList;
                        if (SettingsService.LiveTile)
                            ChangeTile(TopList);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }

        private async Task LoadCachedTop()
        {
            var appData = ApplicationData.Current.LocalFolder;
            try
            {
                TopList = new ObservableCollection<FourItem>();
                var file = await appData.GetFileAsync(TOP);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var xdoc = XDocument.Load(stream);
                    foreach (var xsource in xdoc.Descendants("Top"))
                    {
                        TopList.Add(
                            new FourItem
                            {
                                CommentLink = xsource.Element("CommentLink").Value,
                                FullLink = xsource.Element("FullLink").Value,
                                Image = xsource.Element("Image").Value,
                                Avatar = xsource.Element("Avatar").Value,
                                Link = xsource.Element("Link").Value,
                                Title = xsource.Element("Title").Value,
                            });
                    }
                }
            }
            catch { }
        }

        private async Task LoadSource()
        {
            try
            {
                var task = Methods.GetSourcesAsync(SOURCES);
                await LoadCachedSource();
                var cachedList = new List<FourSource>();
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    SourceView.ItemsSource = SourceList;
                    cachedList = SourceList.ToList();
                    await LoadHiddenSource();
                    HiddenView.ItemsSource = HiddenList;
                }
                // Renew cache
                try
                {
                    var newSourceList = await task;
                    if (!cachedList.Any() && !newSourceList.Any())
                        throw new Exception("Network is not present at the moment");
                    // Checks lists for equality
                    bool equal = true;
                    if (newSourceList.Count == cachedList.Count)
                    {
                        for (int i = 0; i < newSourceList.Count; i++)
                        {
                            if (newSourceList[i].ToString() != cachedList[i].ToString()) equal = false;
                        }
                    }
                    else equal = false;
                    // Renew if not equal
                    if (!equal && newSourceList.Any())
                    {
                        SourceList = newSourceList.ToObservable();
                        await LoadHiddenSource();
                        SourceView.ItemsSource = SourceList;
                        HiddenView.ItemsSource = HiddenList;
                    }
                }
                catch
                {
                    if (cachedList == null || !cachedList.Any())
                        throw;
                }
            }
            catch (Exception ex)
            {
                var text = String.Format(
@"Невозможно подключиться к серверу FourClient.
Попробуйте, пожалуйста, позже.

Детали:
{0} {1}",
                    ex.Message,
                    ex.StackTrace);
                var dialog = new MessageDialog(text, "Возникла ошибка");
                await dialog.ShowAsync();
            }
        }

        private async Task LoadCachedSource()
        {
            var appData = ApplicationData.Current.LocalFolder;
            try
            {
                SourceList = new ObservableCollection<FourSource>();
                var file = await appData.GetFileAsync(SOURCES);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var xdoc = XDocument.Load(stream);
                    foreach (var xsource in xdoc.Descendants("Source"))
                    {
                        SourceList.Add(
                            new FourSource
                            {
                                Name = xsource.Element("Name").Value,
                                Prefix = xsource.Element("Prefix").Value,
                                ImageUrl = xsource.Element("ImageUrl").Value,
                                Base64Types = xsource.Element("Base64Types").Value,
                                Availability = xsource.Element("Availability").Value,
                                Searchability = xsource.Element("Searchability").Value,
                            });
                    }
                }
            }
            catch { }
        }

        private async Task LoadHiddenSource()
        {
            var hideablePrefixes = new List<string>();
            var appData = ApplicationData.Current.LocalFolder;
            try
            {
                HiddenList = new ObservableCollection<FourSource>();
                var file = await appData.GetFileAsync(HIDDEN);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var xdoc = XDocument.Load(stream);
                    foreach (var xsource in xdoc.Descendants("Source"))
                    {
                        hideablePrefixes.Add(xsource.Element("Prefix").Value);
                    }
                }
                var hideableSources = SourceList.Where(s => hideablePrefixes.Contains(s.Prefix)).ToList();
                foreach (var source in hideableSources)
                {
                    SourceList.Remove(source);
                    HiddenList.Add(source);
                }
            }
            catch { }
            HiddenGrid.Opacity = HiddenList.Any() ? 1 : 0.25;
            HiddenGrid.IsHitTestVisible = HiddenList.Any() ? true : false;
        }

        private async void PageList_ServiceExceptionThrown(Exception ex)
        {
            FeedRing.IsActive = false;
            var dialog = new MessageDialog(ex.Message, "WebServiceClient.ServiceException");
            await dialog.ShowAsync();
        }

        private async void PageCollection_ConnectionExceptionThrown(Exception ex)
        {
            FeedRing.IsActive = false;
            var dialog = new MessageDialog(ex.Message, "ConnectionException");
            await dialog.ShowAsync();
        }

        private void LoadSearch(string searchQuery)
        {
            PageCollection = new PageCollection
            {
                SearchQuery = searchQuery,
                SearchMode = true,
                Source = _source
            };
            PageCollection.LoadFailed += PageList_LoadFailed;
            PageCollection.ServiceExceptionThrown += PageList_ServiceExceptionThrown;
            PageCollection.LoadStarted += () => MainPage.StatusProgress(true);
            PageCollection.LoadCompleted += () => MainPage.StatusProgress(false);
            FeedView.ItemsSource = PageCollection;
        }

        private async void PageList_LoadFailed()
        {
            FeedCaption.Visibility = Visibility.Collapsed;
            FeedRing.IsActive = false;
            var dialog = new MessageDialog("Попробовать еще раз?", "Проблемы с сетью");
            dialog.Commands.Add(new UICommand("Да", new UICommandInvokedHandler(YesHandler)));
            dialog.Commands.Add(new UICommand("Нет", new UICommandInvokedHandler(NoHandler)));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
        }
        private async void YesHandler(IUICommand command)
        {
            PageCollection.HasMoreItems = true;
            await PageCollection.LoadMoreItemsAsync(0);
        }
        private void NoHandler(IUICommand command)
        {
            FeedView.ItemsSource = new ObservableCollection<FourItem>();
            PivotControl.SelectedItem = CollectionTab;
        }

        private async Task LoadCollection()
        {
            var appData = ApplicationData.Current.LocalFolder;
            try
            {
                var file = await appData.GetFileAsync(COLLECTION);
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var xdoc = XDocument.Load(stream);
                    PageList = new ObservableCollection<FourItem>();
                    foreach (var xpage in xdoc.Descendants("Page"))
                    {
                        PageList.Add(
                            new FourItem
                            {
                                Title = xpage.Element("Title").Value,
                                Image = xpage.Element("Image").Value,
                                Link = xpage.Element("Link").Value,
                                FullLink = xpage.Element("FullLink").Value,
                                CommentLink = xpage.Element("CommentLink").Value,
                            });
                    }
                }
            }
            catch
            {
                PageList = new ObservableCollection<FourItem>();
            }
            CollectionView.ItemsSource = PageList;
            if (PageList != null && PageList.Any())
                CollectionCaption.Visibility = Visibility.Collapsed;
        }
        #endregion

        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_source == null) return;
            var grid = sender as Grid;
            _dataContext = (FourItem)grid.DataContext;
            var page = _dataContext;
            if (_flyoutOpened) return;
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(animation, grid);
            Storyboard.SetTargetProperty(animation, "(UIElement.Projection).(PlaneProjection.RotationX)");
            AddToHistory(_source.Prefix, page);
            await Task.Delay(250);
            MainPage.GoToArticle(_source.Prefix, page.Title, page.Link, page.FullLink, page.CommentLink, page.Title);
        }

        private void CollectionGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            _dataContext = (FourItem)grid.DataContext;
            var page = _dataContext;
            if (_flyoutOpened) return;
            var args = _dataContext.Link.Split(';').ToList();
            while (args.Count() > 2)
            {
                args[1] += ';' + args[2];
                args.Remove(args[2]);
            }
            AddToHistory(args[0], page);
            MainPage.GoToArticle(args[0], page.Title, args[1], page.FullLink, page.CommentLink, page.Title);
        }

        private async void AddToHistory(string prefix, FourItem page)
        {
            var link = prefix == "NEW" ?
                    page.Link :
                    prefix + ';' + page.Link;
            if (HistoryList.Any(h => h.Link == link))
                return;
            var newPage = new FourItem
            {
                Title = page.Title,
                Link = link
            };
            HistoryList.Insert(0, newPage);
            if (HistoryList.Count > 10)
                HistoryList.RemoveAt(10);
            var xroot = new XElement("Collection");
            var xdoc = new XDocument(xroot);
            foreach (var historyPage in HistoryList)
            {
                var xsource =
                    new XElement("History",
                        new XElement("Link", historyPage.Link),
                        new XElement("Title", historyPage.Title));
                xroot.Add(xsource);
            };
            var appData = ApplicationData.Current.LocalFolder;
            var fileCreate = appData.CreateFileAsync(HISTORY, CreationCollisionOption.ReplaceExisting);
            var file = await fileCreate;
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var bytes = Encoding.UTF8.GetBytes(xdoc.ToString());
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public void BackPressed()
        {
            if (Flyout.Visibility == Visibility.Visible)
            {
                HideFlyout();
                return;
            }
            if (HoverGrid.Visibility == Visibility.Visible)
            {
                HideHoverListView();
                return;
            }
            if (AppBarMenu.Visibility == Visibility.Visible)
            {
                AppBar_ToggleState();
                return;
            }
            if (PivotControl.SelectedItem != SourceTab)
            {
                PivotControl.SelectedItem = SourceTab;
                return;
            }
            App.Current.Exit();
        }

        private async void BlinkAsync()
        {
            rectangle.Visibility = Visibility.Visible;
            await Blink.PlayAsync();
            rectangle.Visibility = Visibility.Collapsed;
        }

        #region AppBar
        private async void AppBar_ToggleState(object sender = null, TappedRoutedEventArgs e = null)
        {
            if (_sliding) return;
            _sliding = true;
            var height = AppBarMenu.ActualHeight;
            if (AppBarMenu.Visibility == Visibility.Visible)
            {
                HideHeight.Value = height;
                await HideMenu.PlayAsync();
                AppBarMenu.Visibility = Visibility.Collapsed;
                BlinderRectangle.Visibility = Visibility.Collapsed;
            }
            else
            {
                AppBarMenu.Visibility = Visibility.Visible;
                BlinderRectangle.Visibility = Visibility.Visible;
                RaiseHeight.Value = height;
                RaiseMenu.Begin();
            }
            _sliding = false;
        }

        private void AppBar_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var threshold = 3;
            if (e.Delta.Translation.Y < -threshold && AppBarMenu.Visibility == Visibility.Collapsed)
                AppBar_ToggleState();
            if (e.Delta.Translation.Y > threshold && AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
        }
        #endregion

        #region Tiles
        private void ChangeTile(ObservableCollection<FourItem> pageList)
        {
            if (pageList.Count(p => !(String.IsNullOrEmpty(p.Image))) < 3) return;
            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueue(false);
                var notifications = updater.GetScheduledTileNotifications();
                foreach (var item in notifications)
                {
                    updater.RemoveFromSchedule(item);
                }
                updater.StopPeriodicUpdate();
                updater.Clear();
                updater.EnableNotificationQueue(true);
                var list = pageList.Where(p => !(String.IsNullOrEmpty(p.Image))).Reverse();
                foreach (var page in list)
                {
                    UpdateTile(updater, page);
                }
            }
            catch { }
        }

        private static FourItem UpdateTile(TileUpdater updater, FourItem item)
        {
            ITileSquare310x310Image tileContent = TileContentFactory.CreateTileSquare310x310Image();
            tileContent.Image.Src = item.Image;
            tileContent.Image.Alt = "310x310";
            ITileWide310x150Image wide310x150Content = TileContentFactory.CreateTileWide310x150Image();
            wide310x150Content.Image.Src = item.Image;
            wide310x150Content.Image.Alt = "310x150";
            ITileSquare150x150Image square150x150Content = TileContentFactory.CreateTileSquare150x150Image();
            square150x150Content.Image.Src = item.Image;
            square150x150Content.Image.Alt = "150x150";
            wide310x150Content.Square150x150Content = square150x150Content;
            tileContent.Wide310x150Content = wide310x150Content;
            updater.Update(tileContent.CreateNotification());
            return item;
        }
        #endregion

        #region Flyout
        private void flyout_Opening(object sender, object e)
        {
            _flyoutOpened = true;
        }

        private void flyout_Closed(object sender, object e)
        {
            _flyoutOpened = false;
        }

        private async void pin_Tapped(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            if (_source == null) return;
            var page = _dataContext;
            var tileID = page.Title.GetHashCode().ToString();
            var imageUri = new Uri("ms-appx:///Assets/Logo.scale-240.png");
            var secondaryTile = new SecondaryTile(
                tileID,
                page.Title,
                _source.Prefix + ';' + page.Link,
                imageUri,
                TileSize.Square150x150);
            await secondaryTile.RequestCreateAsync();
            var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID);
            UpdateTile(updater, page);
        }

        private async void pin_SourceTapped(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            if (_sourceContext == null) return;
            var source = _sourceContext;
            var tileID = source.Prefix.GetHashCode().ToString();
            var imageUri = new Uri("ms-appx:///Assets/Logo.scale-240.png");
            var secondaryTile = new SecondaryTile(
                tileID,
                source.Name,
                source.Prefix,
                imageUri,
                TileSize.Square150x150);
            await secondaryTile.RequestCreateAsync();
            var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID);
            // Fake page. Bad desigion
            var page = new FourItem { Image = source.ImageUrl };
            UpdateTile(updater, page);
        }

        private async void hide_Click(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            var inSource = _sourceContext;
            SourceList.Remove(inSource);
            HiddenList.Add(inSource);
            var xroot = new XElement("Hidden");
            var xdoc = new XDocument(xroot);
            foreach (var source in HiddenList)
            {
                var xsource =
                    new XElement("Source",
                        new XElement("Prefix", source.Prefix));
                xroot.Add(xsource);
            };
            var appData = ApplicationData.Current.LocalFolder;
            var fileCreate = appData.CreateFileAsync(HIDDEN, CreationCollisionOption.ReplaceExisting);
            var file = await fileCreate;
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var bytes = Encoding.UTF8.GetBytes(xdoc.ToString());
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            HiddenGrid.Opacity = HiddenList.Any() ? 1 : 0.25;
            HiddenGrid.IsHitTestVisible = HiddenList.Any() ? true : false;
        }

        private async void unhide_Click(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            var inSource = _sourceContext;
            SourceList.Add(inSource);
            HiddenList.Remove(inSource);
            var xroot = new XElement("Hidden");
            var xdoc = new XDocument(xroot);
            foreach (var source in HiddenList)
            {
                var xsource =
                    new XElement("Source",
                        new XElement("Prefix", source.Prefix));
                xroot.Add(xsource);
            };
            var appData = ApplicationData.Current.LocalFolder;
            var fileCreate = appData.CreateFileAsync(HIDDEN, CreationCollisionOption.ReplaceExisting);
            var file = await fileCreate;
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var bytes = Encoding.UTF8.GetBytes(xdoc.ToString());
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            if (!HiddenList.Any())
            {
                HiddenGrid.Opacity = 0.25;
                HiddenGrid.IsHitTestVisible = false;
                HiddenToggleButton.IsChecked = false;
            }
        }

        private async void pinCollection_Tapped(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            var page = _dataContext;
            var tileID = page.Title.GetHashCode().ToString();
            var imageUri = new Uri("ms-appx:///Assets/Logo.scale-240.png");
            var secondaryTile = new SecondaryTile(
                tileID,
                page.Title,
                page.Link,
                imageUri,
                TileSize.Square150x150);
            await secondaryTile.RequestCreateAsync();
            var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID);
            UpdateTile(updater, page);
        }

        private async void save_Tapped(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            if (_source == null) return;
            MainPage.StatusProgress(true);
            var page = new FourItem
            {
                Title = _dataContext.Title,
                Image = _dataContext.Image,
                Link = _source.Prefix + ';' + _dataContext.Link,
                FullLink = _dataContext.FullLink,
                CommentLink = _dataContext.CommentLink
            };
            var article = await _source.GetArticleAsync(_dataContext.Link);
            MainPage.StatusProgress(false);
            var xml = new XDocument();
            xml.Add(new XElement("HTML", article.HTML));
            var appData = ApplicationData.Current.LocalFolder;
            if (PageList.Any(p => p.Link == page.Link))
            {
                await new MessageDialog
                (
                    title: "Новость уже сохранена",
                    content: "Слушай, ты уже сохранял эту новость. И память у тебя в телефоне не резиновая, так что не вижу смысла её замусоривать копиями, ок? Но на случай если в статье что-то поменялось, то сохраненную копию я уже обновил."
                ).ShowAsync();
                return;
            }
            var filename = page.Link.GetHashCode().ToString() + ".html";
            var file = await appData.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                xml.Save(stream);
            }
            PageList.Add(page);
            CollectionCaption.Visibility = Visibility.Collapsed;
            await SaveCollection();
            BlinkAsync();
        }

        private async void delete_Tapped(object sender, RoutedEventArgs e)
        {
            HideHoverListView();
            PageList.Remove(_dataContext);
            if (!PageList.Any())
                CollectionCaption.Visibility = Visibility.Visible;
            await SaveCollection();
        }
        #endregion

        #region Share
        private void share_Tapped(object sender, RoutedEventArgs e)
        {
            _shareContext = _dataContext;
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareDataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = _shareContext.Title;
            var item = _shareContext;
            request.Data.Properties.Description = "Отправлено из FourClient для Windows Phone";
            try
            {
                var uri = new Uri(string.Format(SettingsService.ShareTemplate, _source?.Prefix ?? "NEW", item.Link, item.Title));
                request.Data.SetWebLink(uri);
            }
            finally
            {
                deferral.Complete();
            }
        }
        #endregion

        private async Task SaveCollection()
        {
            var xroot = new XElement("Collection");
            var xdoc = new XDocument(xroot);
            foreach (var page in PageList)
            {
                var xpage =
                    new XElement("Page",
                        new XElement("Title", page.Title),
                        new XElement("Image", page.Image),
                        new XElement("Link", page.Link),
                        new XElement("FullLink", page.FullLink),
                        new XElement("CommentLink", page.CommentLink));
                xroot.Add(xpage);
            };
            var appData = ApplicationData.Current.LocalFolder;
            var file = await appData.CreateFileAsync(COLLECTION, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var bytes = Encoding.UTF8.GetBytes(xdoc.ToString());
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private void CollectionGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e?.HoldingState == HoldingState.Completed) return;
            _dataContext = (FourItem)(sender as Grid).DataContext;
            var list = new List<ListViewItem>();
            var pin = new ListViewItem() { Content = "На рабочий стол" };
            pin.Tapped += pinCollection_Tapped;
            list.Add(pin);
            var remove = new ListViewItem() { Content = "Удалить" };
            remove.Tapped += delete_Tapped;
            list.Add(remove);
            var share = new ListViewItem() { Content = "Поделиться" };
            share.Tapped += share_Tapped;
            list.Add(share);
            ShowHoverListView(list, sender as FrameworkElement);
        }

        private void CollectionGrid_Right(object sender, RightTappedRoutedEventArgs e) => CollectionGrid_Holding(sender, null);

        private void Top_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e?.HoldingState == HoldingState.Completed) return;
            _dataContext = (FourItem)(sender as Grid).DataContext;
            var list = new List<ListViewItem>();
            var pin = new ListViewItem() { Content = "На рабочий стол" };
            pin.Tapped += pinCollection_Tapped;
            list.Add(pin);
            var share = new ListViewItem() { Content = "Поделиться" };
            share.Tapped += share_Tapped;
            list.Add(share);
            ShowHoverListView(list, sender as FrameworkElement);
        }

        private void Top_Right(object sender, RightTappedRoutedEventArgs e) => Top_Holding(sender, null);

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e?.HoldingState == HoldingState.Completed) return;
            _dataContext = (FourItem)(sender as Grid).DataContext;
            var list = new List<ListViewItem>();
            var pin = new ListViewItem() { Content = "На рабочий стол" };
            pin.Tapped += pin_Tapped;
            list.Add(pin);
            var save = new ListViewItem() { Content = "Сохранить" };
            save.Tapped += save_Tapped;
            list.Add(save);
            var share = new ListViewItem() { Content = "Поделиться" };
            share.Tapped += share_Tapped;
            list.Add(share);
            ShowHoverListView(list, sender as FrameworkElement);
        }

        private void Grid_Right(object sender, RightTappedRoutedEventArgs e) => Grid_Holding(sender, null);

        private void Source_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e?.HoldingState == HoldingState.Completed) return;
            _sourceContext = (FourSource)(sender as Grid).DataContext;
            var list = new List<ListViewItem>();
            var pin = new ListViewItem() { Content = "На рабочий стол" };
            pin.Tapped += pin_SourceTapped;
            list.Add(pin);
            var hide = new ListViewItem() { Content = "Отключить источник" };
            hide.Tapped += hide_Click;
            list.Add(hide);
            ShowHoverListView(list, sender as FrameworkElement);
        }

        private void Source_Right(object sender, RightTappedRoutedEventArgs e) => Source_Holding(sender, null);

        private void Hidden_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e?.HoldingState == HoldingState.Completed) return;
            _sourceContext = (FourSource)(sender as Grid).DataContext;
            var list = new List<ListViewItem>();
            var unhide = new ListViewItem() { Content = "Включить" };
            unhide.Tapped += unhide_Click;
            list.Add(unhide);
            ShowHoverListView(list, sender as FrameworkElement);
        }

        private void Hidden_Right(object sender, RightTappedRoutedEventArgs e) => Hidden_Holding(sender, null);

        private void Source_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _source = SourceView.SelectedItem as FourSource;
            InnerSourceTap();
        }

        private void Hidden_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _source = HiddenView.SelectedItem as FourSource;
            InnerSourceTap();
        }

        private void InnerSourceTap()
        {
            LoadPage();
            PivotControl.SelectedItem = FeedTab;
            MainPage.SetTitle("FourClient ({0})", _source.Name);
        }

        private async void PivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var animationTask = HideMenu.PlayAsync();
            if (PivotControl.SelectedItem == FeedTab)
            {
                AppBarButtonRefresh.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Visible;
                AppBarButtonTopics.Visibility = Visibility.Visible;
                await Task.Delay(1);
                await RaiseButtons.PlayAsync();
            }
            else
            {
                await HideButtons.PlayAsync();
                AppBarButtonRefresh.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Collapsed;
                AppBarButtonTopics.Visibility = Visibility.Collapsed;
            }
            UpdatePivotControls();
            await animationTask;
            AppBarMenu.Visibility = Visibility.Collapsed;
        }

        private void AddScrollListener(ListViewBase view)
        {
            try
            {
                var scrollViewer = view.GetScrollViewer();
                if (scrollViewer == null) return;
                scrollViewer.ViewChanged += scrollViewer_ViewChanged;
            }
            catch
            {
                //ok
            }
        }

        private void RemoveScrollListener(ListViewBase view)
        {
            try
            {
                var scrollViewer = view.GetScrollViewer();
                if (scrollViewer == null) return;
                scrollViewer.ViewChanged -= scrollViewer_ViewChanged;
            }
            catch
            {
                //ok
            }
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            var newScroll = scrollViewer.VerticalOffset;
            if (!_oldScroll.ContainsKey(scrollViewer))
            {
                _oldScroll.Add(scrollViewer, newScroll);
                return;
            }
            var oldScroll = _oldScroll[scrollViewer];
            _oldScroll[scrollViewer] = newScroll;
            var diff = newScroll - oldScroll;
            if (SettingsService.UpperMenu)
            {
                if (diff > 10)
                    UpperPivotHeader.Visibility = Visibility.Collapsed;
                if (diff < -10)
                    UpperPivotHeader.Visibility = Visibility.Visible;
            }
            else
            {
                if (diff > 10)
                {
                    LeftPivotHeader.Visibility = Visibility.Collapsed;
                    SearchButton.Visibility = Visibility.Collapsed;
                }
                if (diff < -10)
                { 
                    LeftPivotHeader.Visibility = Visibility.Visible;
                    SearchButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdatePivotControls()
        {
            RemoveScrollListener(TopView);
            RemoveScrollListener(SourceView);
            RemoveScrollListener(FeedView);
            RemoveScrollListener(CollectionView);
            SplitInterestingButton.IsChecked = false;
            SplitSourceButton.IsChecked = false;
            SplitFeedButton.IsChecked = false;
            SplitCollectionButton.IsChecked = false;
            if (PivotControl.SelectedItem == InterestingTab)
            {
                LeftPivotHeader.Text = "Интересное";
                SplitInterestingButton.IsChecked = true;
                AddScrollListener(TopView);
            }
            if (PivotControl.SelectedItem == SourceTab)
            {
                LeftPivotHeader.Text = "Источники";
                SplitSourceButton.IsChecked = true;
                AddScrollListener(SourceView);
            }
            if (PivotControl.SelectedItem == FeedTab)
            {
                LeftPivotHeader.Text = "Лента";
                SplitFeedButton.IsChecked = true;
                AddScrollListener(FeedView);
            }
            if (PivotControl.SelectedItem == CollectionTab)
            {
                LeftPivotHeader.Text = "Коллекция";
                SplitCollectionButton.IsChecked = true;
                AddScrollListener(CollectionView);
            }
            UpperPivotHeader.Text = LeftPivotHeader.Text.ToUpper();
            UpperInterestingButton.IsChecked = SplitInterestingButton.IsChecked;
            UpperSourceButton.IsChecked = SplitSourceButton.IsChecked;
            UpperFeedButton.IsChecked = SplitFeedButton.IsChecked;
            UpperCollectionButton.IsChecked = SplitCollectionButton.IsChecked;
            if (!SettingsService.UpperMenu)
                LeftPivotHeader.Visibility = Visibility.Visible;
            else
                UpperPivotHeader.Visibility = Visibility.Visible;
        }

        #region AppBar
        private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RefreshAnimation.Begin();
            e.Handled = true;
            if (_source == null) return;
            PageCollection.Clear();
            LoadPage();
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
        }

        #region Search
        private async void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (_source == null) return;
            searchPopup.Visibility = Visibility.Visible;
            var animationTask = ShowSearch.PlayAsync();
            searchBox.Focus(FocusState.Programmatic);
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            await animationTask;
        }

        private void searchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
                searchBox_LostFocus(sender, e);
            if (e.Key != VirtualKey.Enter) return;
            PageCollection.Clear();
            LoadSearch(searchBox.Text);
            searchBox_LostFocus(sender, e);
        }

        private async void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            await HideSearch.PlayAsync();
            searchBox.Text = string.Empty;
            searchPopup.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void TopicsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_source == null ||
                _source.NewsTypes == null ||
                _source.NewsTypes.Count < 2)
                return;
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            var menuView = new StackPanel();
            var header = new StackPanel { Orientation = Orientation.Horizontal };
            if (!SettingsService.IsPhone)
            {
                var back = new AppBarButton
                {
                    Width = 50,
                    Height = 50,
                    IsCompact = true,
                    Icon = new FontIcon
                    {
                        Glyph = "",
                        FontSize = 16
                    }
                };
                back.Tapped += (s, ev) => HideFlyout();
                header.Children.Add(back);
            }
            var headerText = new TextBlock
            {
                Text = "Разделы",
                FontSize = 24,
                Margin = new Thickness(8)
            };
            header.Children.Add(headerText);
            var listbox = new ListView();
            menuView.Children.Add(header);
            menuView.Children.Add(listbox);
            foreach (var n in _source.NewsTypes.Keys)
            {
                var item = new ListViewItem
                {
                    FontSize = 16,
                    Content = n.ToLower(),
                    Tag = n
                };
                item.Tapped += (s, t) =>
                {
                    HideFlyout();
                    LoadPage((s as FrameworkElement).Tag as string);
                };
                listbox.Items.Add(item);
            }
            ShowFlyout(menuView);
        }

        private void HelpButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            MainPage.GoToAbout();
        }

        private void SettingsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            MainPage.GoToSettings();
        }

        private void HistoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            var menuView = new StackPanel();
            var header = new StackPanel { Orientation = Orientation.Horizontal };
            if (!SettingsService.IsPhone)
            {
                var back = new AppBarButton
                {
                    Width = 50,
                    Height = 50,
                    IsCompact = true,
                    Icon = new FontIcon
                    {
                        Glyph = "",
                        FontSize = 16
                    }
                };
                back.Tapped += (s, ev) => HideFlyout();
                header.Children.Add(back);
            }
            var headerText = new TextBlock
            {
                Text = "История",
                FontSize = 24,
                Margin = new Thickness(8)
            };
            header.Children.Add(headerText);
            var listbox = new ListView();
            menuView.Children.Add(header);
            menuView.Children.Add(listbox);
            foreach (var h in HistoryList)
            {
                var item = new ListViewItem
                {
                    FontSize = 16,
                    Content = h.Title,
                    Tag = h
                };
                item.Tapped += (s, t) =>
                {
                    HideFlyout();
                    var page = (FourItem)item.Tag;
                    MainPage.GoToArticle("NEW", page.Title, page.Link, null, null, null);
                };
                listbox.Items.Add(item);
            }
            ShowFlyout(menuView);
        }

        private void ShowFlyout(UIElement element)
        {
            if (Flyout.Visibility == Visibility.Visible) return;
            Flyout.Children.Add(element);
            Flyout.Visibility = Visibility.Visible;
            Flyout.Animate();
        }

        private void HideFlyout()
        {
            Flyout.Children.Clear();
            Flyout.Visibility = Visibility.Collapsed;
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
        #endregion

        public async void GoToSource(string prefix)
        {
            await _loader;
            if (SourceList == null) return;
            var source = SourceList.FirstOrDefault(s => s.Prefix == prefix);
            if (source == null) return;
            SourceView.SelectedItem = source;
            Source_Tapped(this, new TappedRoutedEventArgs());
        }

        private void HiddenToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            HiddenView.Visibility = Visibility.Visible;
        }

        private void HiddenToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            HiddenView.Visibility = Visibility.Collapsed;
        }

        private async void PaidGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=4c456504-64b5-4084-99fa-2af2c3e71b41"));
        }

        private async void SplitViewButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://windowsphone.uservoice.com/forums/101801-feature-suggestions/suggestions/6623043-stick-to-modern-don-t-copy-android"));
        }

        private void InterestingButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotControl.SelectedItem = InterestingTab;
            e.Handled = true;
            UpdatePivotControls();
        }

        private void SourceButtonButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotControl.SelectedItem = SourceTab;
            e.Handled = true;
            UpdatePivotControls();
        }

        private void FeedButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotControl.SelectedItem = FeedTab;
            e.Handled = true;
            UpdatePivotControls();
        }

        private void CollectionButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotControl.SelectedItem = CollectionTab;
            e.Handled = true;
            UpdatePivotControls();
        }

        private void Source_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var container = SourceView;
            var maxSize = container.ActualWidth - 12;
            var columns = Math.Floor(maxSize / 95);
            var desiredSize = Math.Floor(maxSize / columns) - 1;
            grid.Width = desiredSize;
            grid.Height = desiredSize;
        }

        private void CollectionGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var container = CollectionView;
            var maxSize = container.ActualWidth - 12;
            var columns = Math.Floor(maxSize / 140);
            var desiredSize = Math.Floor(maxSize / columns) - 1;
            grid.Width = desiredSize;
            grid.Height = desiredSize;
        }

        private void FeedGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            FeedChangeGridSize(grid);
            _feedItems.Add(grid);
        }

        private void FeedGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            _feedItems.Remove(grid);
        }

        private void FeedView_SizeChanged(object sender, SizeChangedEventArgs e) => _feedItems.ForEach(FeedChangeGridSize);

        private void FeedChangeGridSize(Grid grid)
        {
            var container = FeedView;
            var maxSize = container.ActualWidth - 4;
            if (maxSize <= 0) return;
            grid.Width = maxSize;
        }

        private void TopGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            TopChangeGridSize(grid);
            _topItems.Add(grid);
        }

        private void TopGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            _topItems.Remove(grid);
        }

        private void TopView_SizeChanged(object sender, SizeChangedEventArgs e) => _topItems.ForEach(TopChangeGridSize);

        private void TopChangeGridSize(Grid grid)
        {
            var container = TopView;
            var maxSize = container.ActualWidth - 4;
            if (maxSize <= 0) return;
            grid.Width = maxSize;
        }

        private void FeedAppBarTop_SizeChanged(object sender, SizeChangedEventArgs e) => PivotControl.Padding = new Thickness(0, 0, 0, e.NewSize.Height);

        private void HoverGrid_Tapped(object sender, TappedRoutedEventArgs e) => HideHoverListView();

        private void HoverGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) => HideHoverListView();
    }
}
