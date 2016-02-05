using FourClient.Data;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public interface IInterestingView
    {
        void SetItemsSource(object source);
    }

    public sealed partial class InterestingView : IInterestingView
    {
        public delegate void ViewEventHandler(IInterestingView sender);

        public static event ViewEventHandler ViewLoaded;
        
        public InterestingView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
        }

        public Source SelectedSource { get; private set; }

        public void SetItemsSource(object source)
        {
            GridView.ItemsSource = source;
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as FeedItem;
            if (item != null)
            {
                var article = Article.BuildNew(item);
                IoC.ArticleView.Open(article);
            }
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ShowMenuOn(sender);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ShowMenuOn(sender);

        private static void ShowMenuOn(object sender)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as FeedItem;
            var article = Article.BuildNew(item);
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
    }
}