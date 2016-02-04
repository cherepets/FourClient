using FourToolkit.UI.Extensions;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public interface ICollectionView
    {
        void SetItemsSource(object source);
    }

    public sealed partial class CollectionView : ICollectionView
    {
        public delegate void ViewEventHandler(ICollectionView sender);

        public static event ViewEventHandler ViewLoaded;

        public CollectionView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
            Loaded += (s, e) => GridView.AttachScrollListener(OnScrollDown, OnScrollUp);
        }

        public void OnScrollDown() => IoC.MenuView.HideBars();
        public void OnScrollUp() => IoC.MenuView.ShowBars();

        public void SetItemsSource(object source)
        {
            var collection = source as ObservableCollection<Article>;
            if (collection == null) return;
            GridView.ItemsSource = collection;
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as Article;
            if (item != null)
                IoC.ArticleView.Open(item);
        }

        private void Item_Holding(object sender, HoldingRoutedEventArgs e)
        {

        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }
    }
}
