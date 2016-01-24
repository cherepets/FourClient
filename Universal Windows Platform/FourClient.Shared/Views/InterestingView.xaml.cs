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
            Loaded += (s, e) => GridView.AttachScrollListener(OnScrollDown, OnScrollUp);
        }

        public void OnScrollDown() => IoC.MenuView.HideBars();
        public void OnScrollUp() => IoC.MenuView.ShowBars();

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
                var args = item.Link.Split(';').ToList();
                while (args.Count() > 2)
                {
                    args[1] += ';' + args[2];
                    args.Remove(args[2]);
                }
                IoC.ArticleView.Open(args[0], args[1], item.FullLink, item.CommentLink, item.Title);
            }
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ShowMenuOn(sender);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ShowMenuOn(sender);

        private static void ShowMenuOn(object sender)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as Source;
            if (item != null)
                ContextMenu.Show(IoC.MainPage.Flyout, panel,
                    new ContextMenuItem("Delete",
                        async () =>
                        {
                            //                          await item.DeleteAsync();
                        })
                    );
        }
    }
}