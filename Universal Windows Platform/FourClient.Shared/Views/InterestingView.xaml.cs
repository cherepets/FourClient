using FourClient.Data;
using FourClient.Library;
using FourToolkit.UI;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public interface IInterestingView
    {
        void Refresh();
        bool EnableFlipViewer { get; set; }
    }

    public sealed partial class InterestingView : IInterestingView
    {
        public delegate void ViewEventHandler(IInterestingView sender);

        public static event ViewEventHandler ViewLoaded;

        public InterestingView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
            Loaded += (s, e) =>
            {
                GridViewFlipper.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                EnableFlipViewer = Settings.Current.EnableFlipViewer;
            };
        }

        public Source SelectedSource { get; private set; }

        private IFlipper Flipper => _enableFlipViewer ? (IFlipper)VerticalFlipper : GridViewFlipper;

        public void Refresh()
        {
            var top = Api.GetTop();
            SetItemsSource(top);
            Notifier.RegenerateDummies();
        }

        private void SetItemsSource(object source)
        {
            var items = source as ObservableCollection<FeedItem>;
            if (Settings.Current.FilterInteresting && items != null)
            {
                var filtered = new FilteredObservableCollection<FeedItem>(items,
                    f => !Settings.Current.HiddenSources.Any(
                        s => f.Link.StartsWith(s)));
                items = filtered;
                if (Settings.Current.LiveTile) Notifier.UpdateMainTile(items);
            }
            var list = items as IList;
            VerticalFlipper.ItemsSource = list;
            GridViewFlipper.ItemsSource = list;
        }

        public bool EnableFlipViewer
        {
            get
            {
                return _enableFlipViewer;
            }
            set
            {
                _enableFlipViewer = value;
                VerticalFlipper.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                GridViewFlipper.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private bool _enableFlipViewer;

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = Flipper.SelectedItem as FeedItem;
            if (item != null)
            {
                var article = Article.BuildNew(item);
                IoC.ArticleView.Open(article);
            }
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ConditionalShow(sender, e.PointerDeviceType != PointerDeviceType.Touch);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ConditionalShow(sender, e.HoldingState != HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Pen);

        private void ConditionalShow(object sender, bool condition)
            => (condition ? ShowMenuOn : (Action<object>)null)?.Invoke(sender);

        private void ShowMenuOn(object sender)
        {
            var item = Flipper.SelectedItem as FeedItem;
            var article = Article.BuildNew(item);
            if (article != null)
                ContextMenu.Show(IoC.MainPage.Flyout, Flipper as FrameworkElement,
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
                        }),
                    new ContextMenuItem("Открыть в браузере", () => article.OpenWeb()),
                    new ContextMenuItem("Комментарии", () => article.OpenComments()),
                    new ContextMenuItem("Поделиться", () => article.Share())
                    );
        }
    }
}