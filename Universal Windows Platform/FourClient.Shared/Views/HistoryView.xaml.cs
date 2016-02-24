using FourClient.Library;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public sealed partial class HistoryView
    {
        public HistoryView()
        {
            InitializeComponent();
            GridView.Loaded += (s, a)
                => GridView.ItemsSource = IoC.ArticleCache.GetHistory();
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
            IoC.MainPage.HideFlyout();
            var panel = (Grid)sender;
            var item = panel.DataContext as Article;
            if (item != null)
                IoC.ArticleView.Open(item);
        }
    }
}
