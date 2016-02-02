using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FourToolkit.UI.Extensions;
using FourClient.Data;
using FourClient.Data.Feed;

namespace FourClient.Views
{
    public interface IFeedView
    {
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

        public void OnScrollDown() => IoC.MenuView.HideBars();
        public void OnScrollUp() => IoC.MenuView.ShowBars();

        public void SetItemsSource(object feed)
        {
            Placeholder.Visibility = Visibility.Collapsed;
            GridView.ItemsSource = feed;
            IoC.MenuView.ShowBars();
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as FeedItem;
            var article = new Article
            {
                Prefix = IoC.SourcesView.SelectedSource.Prefix,
                Title = item.Title,
                Image = item.Image,
                Link = item.Link,
                Avatar = item.Avatar,
                FullLink = item.FullLink,
                CommentLink = item.CommentLink
            };
            if (item != null) IoC.ArticleView.Open(article);
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ShowMenuOn(sender);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ShowMenuOn(sender);

        private static void ShowMenuOn(object sender)
        {
            //var panel = (Grid)sender;
            //var item = panel.DataContext as FeedItem;
            //if (item != null)
            //    ContextMenu.Show(IoC.MainPage.Flyout, panel,
            //        new ContextMenuItem("Download",
            //            async () =>
            //            {
            //                FileOperations.Download(await item.GetDirectUrlAsync(), item.Name.Split(' ').FirstOrDefault(), () => IoC.MenuView.OpenDownloadTab());
            //            }),
            //        new ContextMenuItem("Remote",
            //            async () =>
            //            {
            //                var url = string.Empty;
            //                try
            //                {
            //                    url = await item.GetDirectUrlAsync();
            //                }
            //                catch (Exception exception)
            //                {
            //                    IoC.Logger.Error(exception);
            //                }
            //                if (string.IsNullOrEmpty(url)) return;
            //                var proj = new ProjectionView
            //                {
            //                    Url = url
            //                };
            //                IoC.MainPage.ShowFlyout(proj);
            //            })
            //        );
        }

        private void SourcesButton_Tapped(object sender, TappedRoutedEventArgs e)
            => IoC.MenuView.OpenSourcesTab();

        public void Refresh()
        {
            var feed = GridView.ItemsSource as AbstractFeed;
            if (feed != null)
                GridView.ItemsSource = feed.Clone();
        }
    }
}
