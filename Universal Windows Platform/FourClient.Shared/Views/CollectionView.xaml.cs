﻿using FourClient.Library;
using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
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
            var item = panel.DataContext as Article;
            if (item != null)
                IoC.ArticleView.Open(item);
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e) => ConditionalShow(sender, e.PointerDeviceType != PointerDeviceType.Touch);

        private void Item_Holding(object sender, HoldingRoutedEventArgs e) => ConditionalShow(sender, e.HoldingState != HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Pen);

        private void ConditionalShow(object sender, bool condition)
            => (condition ? ShowMenuOn : (Action<object>)null)?.Invoke(sender);

        private static void ShowMenuOn(object sender)
        {
            var panel = (Grid)sender;
            var article = panel.DataContext as Article;
            if (article != null)
                ContextMenu.Show(IoC.MainPage.Flyout, panel,
                    new ContextMenuItem("Открыть в браузере", () => article.OpenWeb()),
                    new ContextMenuItem("Комментарии", () => article.OpenComments()),
                    new ContextMenuItem("Поделиться", () => article.Share()),
                    new ContextMenuItem("Удалить",
                        () =>
                        {
                            var existent = IoC.ArticleCache.FindInCollection(article.Prefix, article.Link)
                                ?? IoC.ArticleCache.FindInCache(article.Prefix, article.Link);
                            if (existent != null)
                            {
                                if (existent.InCollection)
                                {
                                    existent.InCollection = false;
                                    IoC.ArticleCache.UpdateCollectionState(existent);
                                }
                            }
                        })
                    );
        }
    }
}
