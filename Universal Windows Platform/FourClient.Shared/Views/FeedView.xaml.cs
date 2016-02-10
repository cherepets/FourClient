using FourClient.Data;
using FourClient.Data.Feed;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public interface IFeedView
    {
        Source CurrentSource { get; }
        void SetItemsSource(object source);
        void Refresh();
    }

    public sealed partial class FeedView : IFeedView
    {
        public delegate void ViewEventHandler(IFeedView sender);

        public static event ViewEventHandler ViewLoaded;

        public FeedView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
            Loaded += (s, e) => GridView.AttachScrollListener(OnScrollDown, OnScrollUp);
        }

        public Source CurrentSource { get; private set; }

        public void OnScrollDown() => IoC.MenuView.HideBars();
        public void OnScrollUp() => IoC.MenuView.ShowBars();

        public void SetItemsSource(object source)
        {
            Placeholder.Visibility = Visibility.Collapsed;
            var feed = source as AbstractFeed;
            if (feed == null) return;
            feed.LoadStarted += Feed_LoadStarted;
            feed.LoadCompleted += Feed_LoadCompleted;
            feed.LoadFailed += Feed_LoadFailed;
            feed.ConnectionExceptionThrown += Feed_ConnectionExceptionThrown;
            feed.ServiceExceptionThrown += Feed_ServiceExceptionThrown;
            GridView.ItemsSource = feed;
            CurrentSource = feed.Source;
            IoC.MenuView.NewsTypes = feed.Source.Topics.Keys.ToList();
            if (feed.Source != null && feed.Source.Searchable) IoC.MenuView.ShowSearchButton();
            else IoC.MenuView.HideSearchButton();
            IoC.MenuView.ShowBars();
        }

        private void Feed_LoadStarted() => StatusBar.ProgressValue = -1;

        private void Feed_LoadCompleted() => StatusBar.ProgressValue = null;

        private void Feed_LoadFailed() => StatusBar.ProgressValue = null;

        private void Feed_ServiceExceptionThrown(Exception exception) => App.HandleException(exception);

        private async void Feed_ConnectionExceptionThrown(Exception exception)
            => await new MessageDialog("Попробовать еще раз?", "Проблемы с сетью")
                .WithCommand("Да", () => Reconnect())
                .WithCommand("Нет")
                .SetDefaultCommandIndex(0)
                .SetCancelCommandIndex(1)
                .ShowAsync();

        private async void Reconnect()
        {
            var feed = GridView.ItemsSource as AbstractFeed;
            if (feed == null) return;
            feed.HasMoreItems = true;
            await feed.LoadMoreItemsAsync(0);
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid == null) return;
            grid.Width = GridView.ActualWidth;
            GridView.SizeChanged += (s, a) => grid.Width = GridView.ActualWidth;
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as FeedItem;
            var article = Article.Build(item);
            if (article != null) IoC.ArticleView.Open(article);
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ShowMenuOn(sender);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ShowMenuOn(sender);

        private static void ShowMenuOn(object sender)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as FeedItem;
            var article = Article.Build(item);
            if (article != null)
                ContextMenu.Show(IoC.MainPage.Flyout, panel,
                    new ContextMenuItem("В коллекцию",
                        async () =>
                        {
                            var existent = IoC.ArticleCache.FindInCollection(article.Prefix, article.Link)
                                ?? IoC.ArticleCache.FindInCache(article.Prefix, article.Link);
                            if (existent != null)
                            {
                                if (!existent.InCollection)
                                {
                                    existent.InCollection = true;
                                    IoC.ArticleCache.UpdateCollectionState(existent);
                                }
                            }
                            else
                            {
                                article.Html = await Api.GetArticleAsync(article.Prefix, article.Link);
                                article.InCollection = true;
                                IoC.ArticleCache.Put(article);
                            }
                        })
                    );
        }

        private void SourcesButton_Tapped(object sender, TappedRoutedEventArgs e)
            => IoC.MenuView.OpenSourcesTab();

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args) => Refresh();

        public void Refresh()
        {
            var feed = GridView.ItemsSource as AbstractFeed;
            if (feed == null) return;
            GridView.ItemsSource = feed.Clone();
        }
    }
}
