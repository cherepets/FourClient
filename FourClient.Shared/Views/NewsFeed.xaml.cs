using FourAPI;
using FourAPI.Types;
using FourClient.Extensions;
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
using Windows.UI.Input;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace FourClient.Views
{
    public sealed partial class NewsFeed : UserControl, IBackButton
    {

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

        public NewsFeed()
        {
            this.InitializeComponent();
            _loader = Load();
        }
        
        public void InvalidateBindings()
        {
            CollectionView.ItemsSource = null;
            FeedView.ItemsSource = null;
            SourceView.ItemsSource = null;
            HiddenView.ItemsSource = null;
            TopView.ItemsSource = null;
            CollectionView.ItemsSource = PageList;
            FeedView.ItemsSource = PageCollection;
            SourceView.ItemsSource = SourceList;
            HiddenView.ItemsSource = HiddenList;
            TopView.ItemsSource = TopList;
        }

        #region Load
        private async Task Load()
        {
            var collectionTask = LoadCollection();
            var sourceTask = LoadSource();
            var topTask = LoadTop();
            var historyTask = LoadHistory();
            await collectionTask;
            await sourceTask;
            await topTask;
            await historyTask;
            AfterLoad();
        }

        private void LoadPage(string newsType = null)
        {
            if (_source == null) return;
            appBarButtonRefresh.IsEnabled = true;
            appBarButtonSearch.IsEnabled = _source.Searchable;
            appBarButtonHamburger.IsEnabled = _source.NewsTypes.Count > 1;
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

        void PageList_LoadFailed()
        {
            FeedCaption.Visibility = Visibility.Collapsed;
            FeedRing.IsActive = false;
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
            MainPage.GoToArticle(_source.Prefix, page.Title, page.Link, page.FullLink, page.CommentLink);
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
            MainPage.GoToArticle(args[0], page.Title, args[1], page.FullLink, page.CommentLink);
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
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            else
            {
                if (PivotControl.SelectedItem != SourceTab)
                    PivotControl.SelectedItem = SourceTab;
                else
                    App.Current.Exit();
            }
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
        }

        private void GridTitle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var threshold = 5;
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
            request.Data.Properties.Description = "Отправлено из FourClient для Windows Phone";
            try
            {
                var uri = new Uri(_shareContext.FullLink);
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
            if (e.HoldingState == HoldingState.Completed) return;
            _dataContext = (FourItem)(sender as Grid).DataContext;
            var flyout = new MenuFlyout();
            var pin = new MenuFlyoutItem() { Text = "на рабочий стол" };
            pin.Click += pinCollection_Tapped;
            flyout.Items.Add(pin);
            var remove = new MenuFlyoutItem() { Text = "удалить" };
            remove.Click += delete_Tapped;
            flyout.Items.Add(remove);
            var share = new MenuFlyoutItem() { Text = "поделиться" };
            share.Click += share_Tapped;
            flyout.Items.Add(share);
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void Top_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed) return;
            _dataContext = (FourItem)(sender as Grid).DataContext;
            var flyout = new MenuFlyout();
            var pin = new MenuFlyoutItem() { Text = "на рабочий стол" };
            pin.Click += pinCollection_Tapped;
            flyout.Items.Add(pin);
            var share = new MenuFlyoutItem() { Text = "поделиться" };
            share.Click += share_Tapped;
            flyout.Items.Add(share);
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed) return;
            _dataContext = (FourItem)(sender as Grid).DataContext;
            var flyout = new MenuFlyout();
            var pin = new MenuFlyoutItem() { Text = "на рабочий стол" };
            pin.Click += pin_Tapped;
            flyout.Items.Add(pin);
            var save = new MenuFlyoutItem() { Text = "сохранить" };
            save.Click += save_Tapped;
            flyout.Items.Add(save);
            var share = new MenuFlyoutItem() { Text = "поделиться" };
            share.Click += share_Tapped;
            flyout.Items.Add(share);
            flyout.ShowAt(sender as FrameworkElement);
        }
        private void Source_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed) return;
            _sourceContext = (FourSource)(sender as Grid).DataContext;
            var flyout = new MenuFlyout();
            var pin = new MenuFlyoutItem() { Text = "на рабочий стол" };
            pin.Click += pin_SourceTapped;
            flyout.Items.Add(pin);
            var hide = new MenuFlyoutItem() { Text = "отключить источник" };
            hide.Click += hide_Click;
            flyout.Items.Add(hide);
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void Hidden_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed) return;
            _sourceContext = (FourSource)(sender as Grid).DataContext;
            var flyout = new MenuFlyout();
            var unhide = new MenuFlyoutItem() { Text = "включить" };
            unhide.Click += unhide_Click;
            flyout.Items.Add(unhide);
            flyout.ShowAt(sender as FrameworkElement);
        }

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

        private void Source_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumns(sender, SourceView, SettingsService.IsPhablet ? 4 : 3);
        }

        private void Hidden_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumns(sender, HiddenView, SettingsService.IsPhablet ? 4 : 3);
        }

        private void Feed_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumns(sender, FeedView, 1, true);
            var grid = sender as FrameworkElement;
            grid.Animate();
        }

        private void Collection_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumns(sender, CollectionView, SettingsService.IsPhablet ? 3 : 2);
            var grid = sender as FrameworkElement;
            grid.Animate();
        }

        private void Top_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var offset = grid.Margin.Top + grid.Margin.Bottom;
            grid.Width = TopView.ActualWidth;
            grid.Height = InterestingTab.ActualHeight / 2 - offset;
            grid.Animate();
        }

        private void SetColumns(object sender, FrameworkElement parent, int columns, bool ignoreHeight = false)
        {
            var grid = sender as FrameworkElement;
            var offset = grid.Margin.Left + grid.Margin.Right;
            var fullWidth = parent.ActualWidth - columns * offset;
            var size = Math.Floor(fullWidth / columns);
            grid.Width = size;
            if (!ignoreHeight)
                grid.Height = size;
        }

        private async void PivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var awaiter = HideMenu.PlayAsync();
            if (PivotControl.SelectedItem == FeedTab)
            {
                appBarButtonRefresh.Visibility = Visibility.Visible;
                appBarButtonSearch.Visibility = Visibility.Visible;
                appBarButtonHamburger.Visibility = Visibility.Visible;
                await Task.Delay(1);
                OffsetRectangle.Height = FeedAppBarTop.ActualHeight;
                await RaiseButtons.PlayAsync();
            }
            else
            {
                OffsetRectangle.Height = 20;
                await HideButtons.PlayAsync();
                appBarButtonRefresh.Visibility = Visibility.Collapsed;
                appBarButtonSearch.Visibility = Visibility.Collapsed;
                appBarButtonHamburger.Visibility = Visibility.Collapsed;
            }
            await awaiter;
            AppBarMenu.Visibility = Visibility.Collapsed;
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
            await SearchAnimation.PlayAsync();
            e.Handled = true;
            if (_source == null) return;
            searchPopup.Visibility = Visibility.Visible;
            await Task.Delay(1);
            searchBox.Focus(FocusState.Programmatic);
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
        }

        private void searchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            PageCollection.Clear();
            LoadSearch(searchBox.Text);
            searchBox_LostFocus(sender, e);
        }

        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            searchBox.Text = String.Empty;
            searchPopup.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void HamburgerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_source == null ||
                _source.NewsTypes == null ||
                _source.NewsTypes.Count < 2)
                return;
            HamburgerAnimation.Begin();
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            var menuView = new StackPanel
            {
                Margin = new Thickness(8, 0, 0, 0)
            };
            var header = new TextBlock
            {
                Text = "Разделы",
                FontSize = 32,
                Margin = new Thickness(8)
            };
            var listbox = new ListView();
            menuView.Children.Add(header);
            menuView.Children.Add(listbox);
            var flyout = new Flyout
            {
                Content = new Rectangle
                {
                    Height = this.ActualHeight
                }
            };
            flyout.Opened += async (s, t) =>
            {
                await Task.Delay(400);
                flyout.Content = menuView;
            };
            foreach (var n in _source.NewsTypes.Keys)
            {
                var item = new ListViewItem
                {
                    FontSize = 24,
                    Margin = new Thickness(8),
                    Content = n.ToLower(),
                    Tag = n
                };
                item.Tapped += (s, t) =>
                {
                    flyout.Hide();
                    LoadPage((s as FrameworkElement).Tag as string);
                };
                listbox.Items.Add(item);
            }
            flyout.ShowAt(FeedAppBarTop);
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
            HamburgerAnimation.Begin();
            if (AppBarMenu.Visibility == Visibility.Visible)
                AppBar_ToggleState();
            var menuView = new StackPanel
            {
                Margin = new Thickness(8, 0, 0, 0)
            };
            var header = new TextBlock
            {
                Text = "История",
                FontSize = 32,
                Margin = new Thickness(8)
            };
            var listbox = new ListView();
            menuView.Children.Add(header);
            menuView.Children.Add(listbox);
            var flyout = new Flyout
            {
                Content = new Rectangle
                {
                    Height = this.ActualHeight
                }
            };
            flyout.Opened += async (s, t) =>
            {
                await Task.Delay(400);
                flyout.Content = menuView;
            };
            foreach (var h in HistoryList)
            {
                var item = new ListViewItem
                {
                    FontSize = 24,
                    Margin = new Thickness(8),
                    Content = h.Title,
                    Tag = h
                };
                item.Tapped += (s, t) =>
                {
                    flyout.Hide();
                    var page = (FourItem)item.Tag;
                    MainPage.GoToArticle("NEW", page.Title, page.Link, null, null);
                };
                listbox.Items.Add(item);
            }
            flyout.ShowAt(FeedAppBarTop);
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

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            HiddenView.Visibility = Visibility.Visible;
            HiddenButtonCheckedAnimation.Begin();
        }

        private void HiddenToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            HiddenView.Visibility = Visibility.Collapsed;
            HiddenButtonUncheckedAnimation.Begin();
        }

        private async void PaidGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=4c456504-64b5-4084-99fa-2af2c3e71b41"));
        }
    }
}
