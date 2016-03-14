using FourClient.Data;
using FourClient.Data.Interfaces;
using FourClient.Library;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public delegate void SourceChangedEventHandler(Source newSource);

    public interface ISourcesView : ISourceSelector
    {
        void SetItemsSource(object source);
    }

    public sealed partial class SourcesView : ISourcesView
    {
        public delegate void ViewEventHandler(ISourcesView sender);

        public static event ViewEventHandler ViewLoaded;

        public SourcesView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
        }

        public IEnumerable<Source> Sources { get; private set; } = new List<Source>();

        public void SetItemsSource(object source)
        {
            var collection = source as ObservableCollection<Source>;
            if (collection == null) return;
            var hiddenSources = Settings.Current.HiddenSources;
            var active = new FilteredObservableCollection<Source>(collection, s => !hiddenSources.Any(p => p == s.Prefix));
            var inactive = new FilteredObservableCollection<Source>(collection, s => hiddenSources.Any(p => p == s.Prefix));
            hiddenSources.CollectionChanged += (s, a) =>
            {
                active.Recheck();
                inactive.Recheck();
                RefreshSources(active);
            };
            GridView.ItemsSource = active;
            HiddenView.ItemsSource = inactive;
            RefreshSources(active);
        }

        private void RefreshSources(FilteredObservableCollection<Source> active)
            => Sources = active.Where(q => q.Prefix != "NEW").ToList();

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            grid.ResizeViewItem(GridView, ResizeMode.Both, 90);
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = (Grid)sender;
            var source = (Source)grid.DataContext;
            IoC.MenuView.OpenFeedTab();
            IoC.FeedView.SetItemsSource(source.MainFeed);
            HideHiddenSources();
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ConditionalShow(sender, e.PointerDeviceType != PointerDeviceType.Touch);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ConditionalShow(sender, e.HoldingState != HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Pen);

        private void HiddenItem_RightTapped(object sender, RightTappedRoutedEventArgs e) => ConditionalShowHidden(sender, e.PointerDeviceType != PointerDeviceType.Touch);

        private void HiddenItem_Holding(object sender, HoldingRoutedEventArgs e) => ConditionalShowHidden(sender, e.HoldingState != HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Pen);

        private void ConditionalShow(object sender, bool condition)
            => (condition ? ShowMenuOn : (Action<object>)null)?.Invoke(sender);

        private static void ShowMenuOn(object sender)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as Source;
            if (item != null)
                ContextMenu.Show(IoC.MainPage.Flyout, panel,
                    new ContextMenuItem("Отключить",
                        () =>
                        {
                            if (!Settings.Current.HiddenSources.Contains(item.Prefix))
                                Settings.Current.HiddenSources.Add(item.Prefix);
                        })
                    );
        }
        
        private void ConditionalShowHidden(object sender, bool condition)
            => (condition ? ShowMenuOnHidden : (Action<object>)null)?.Invoke(sender);

        private static void ShowMenuOnHidden(object sender)
        {
            var panel = (Grid)sender;
            var item = panel.DataContext as Source;
            if (item != null)
                ContextMenu.Show(IoC.MainPage.Flyout, panel,
                    new ContextMenuItem("Включить",
                        () =>
                        {
                            if (Settings.Current.HiddenSources.Contains(item.Prefix))
                                Settings.Current.HiddenSources.Remove(item.Prefix);
                        })
                    );
        }

        private bool HiddenShown => HiddenView.Visibility == Visibility.Visible;

        private void SmallUpArrow_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (HiddenShown) HideHiddenSources();
            else ShowHiddenSources();
        }

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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => HiddenView.MaxHeight = ActualHeight > 128 ? ActualHeight - 128 : ActualHeight;
    }
}