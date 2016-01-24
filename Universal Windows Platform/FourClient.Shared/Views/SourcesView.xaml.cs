using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using FourClient.Data;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace FourClient.Views
{
    public delegate void SourceChangedEventHandler(Source newSource);

    public interface ISourcesView
    {
        Source SelectedSource { get; }
        void SetItemsSource(object source);
        event SourceChangedEventHandler SourceChanged;
    }

    public sealed partial class SourcesView : ISourcesView
    {
        public delegate void ViewEventHandler(ISourcesView sender);

        public static event ViewEventHandler ViewLoaded;

        public event SourceChangedEventHandler SourceChanged;

        public SourcesView()
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
            var collection = source as ObservableCollection<Source>;
            if (collection == null) return;
            var active = new FilteredObservableCollection<Source>(collection, s => s.Name?.StartsWith("W") ?? false);
            var inactive = new FilteredObservableCollection<Source>(collection, s => !(s.Name?.StartsWith("W") ?? false));
            GridView.ItemsSource = active;
            HiddenView.ItemsSource = inactive;
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            grid.ResizeViewItem(GridView, ResizeMode.Both, 90);
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = (Grid)sender;
            var source = (Source)grid.DataContext;
            SelectedSource = source;
            IoC.MenuView.OpenFeedTab();
            IoC.FeedView.SetItemsSource(source.MainFeed);
            IoC.MenuView.NewsTypes = source.NewsTypes.Keys.ToList();
            if (SelectedSource != null && SelectedSource.Searchable) IoC.MenuView.ShowSearchButton();
            else IoC.MenuView.HideSearchButton();
            SourceChanged?.Invoke(source);
            HideHiddenSources();
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

        private void HiddenToggleButton_Checked(object sender, RoutedEventArgs e) => ShowHiddenSources();

        private void HiddenToggleButton_Unchecked(object sender, RoutedEventArgs e) => HideHiddenSources();

        private async void ShowHiddenSources()
        {
            if (HiddenView.Visibility == Visibility.Visible) return;
            HiddenView.Visibility = Visibility.Visible;
            await HiddenView.TranslateByYAsync(TimeSpan.FromMilliseconds(200), 400, 0);
        }

        private async void HideHiddenSources()
        {
            if (HiddenView.Visibility == Visibility.Collapsed) return;
            await HiddenView.TranslateByYAsync(TimeSpan.FromMilliseconds(200), 0, HiddenView.ActualHeight);
            HiddenView.Visibility = Visibility.Collapsed;
        }
    }
}