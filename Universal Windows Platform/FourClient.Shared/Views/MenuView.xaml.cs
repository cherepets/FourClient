using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public interface IMenuView
    {
        void OpenInterestingTab();
        void OpenSourcesTab();
        void OpenFeedTab();
        void OpenCollectionTab();
        void ShowSearchButton();
        void HideSearchButton();
        void ShowBars();
        void HideBars();
        List<string> NewsTypes { get; set; }
    }

    public sealed partial class MenuView : IMenuView
    {
        public delegate void ViewEventHandler(IMenuView sender);
        public static event ViewEventHandler ViewLoaded;

        public MenuView()
        {
            InitializeComponent();
            ViewLoaded?.Invoke(this);
            RefreshHeader(0);
        }

        public List<string> NewsTypes
        {
            get
            {
                return _newsTypes;
            }
            set
            {
                _newsTypes = value;
                NewsTypeButton.Content = _newsTypes.FirstOrDefault() ?? string.Empty;
            }
        }
        private List<string> _newsTypes;

        public void OpenInterestingTab() => Pivot.SelectedItem = InterestingView;
        public void OpenSourcesTab() => Pivot.SelectedItem = SourcesView;
        public void OpenFeedTab() => Pivot.SelectedItem = FeedView;
        public void OpenCollectionTab() => Pivot.SelectedItem = CollectionView;

        public void ShowSearchButton() => SearchButton.Visibility = Visibility.Visible;
        public void HideSearchButton() => SearchButton.Visibility = Visibility.Collapsed;

        private object _lock = new object();

        public async void ShowBars()
        {
            RefreshHeader(Pivot.SelectedIndex);
            await MultiAppBar.ShowAsync();
        }

        public async void HideBars()
        {
            PivotHeader.Visibility = Visibility.Collapsed;
            await MultiAppBar.HideAsync();
        }

        public bool HandleBackButton()
        {
            if (Flyout.IsOpen)
            {
                Flyout.HideFlyout();
                return true;
            }
            if (Pivot.SelectedIndex > 0)
            {
                Pivot.SelectedIndex--;
                return true;
            }
            return false;
        }

        private void RefreshHeader(int index)
        {
            var text = (Pivot.Items[index] as PivotItem)?.Header?.ToString()?.ToUpper();
            if (string.IsNullOrEmpty(text))
                PivotHeader.Visibility = Visibility.Collapsed;
            else
            {
                PivotHeader.Visibility = Visibility.Visible;
                PivotHeader.Text = text;
            }
        }

        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IoC.SourcesView.SelectedSource == null) return;
            var searchInput = new TextInput { VerticalAlignment = VerticalAlignment.Top, Placeholder = "search" };
            Flyout.ShowFlyout(searchInput);
            searchInput.SetFocus();
            searchInput.InputCanceled += () => Flyout.HideFlyout();
            searchInput.InputCompleted += input =>
            {
                //var feed = IoC.SourcesView.SelectedSource.SearchFeed[input];
                //IoC.FeedView.SetItemsSource(feed);
                IoC.MenuView.OpenFeedTab();
                Flyout.HideFlyout();
            };
        }

        private void HorizontalSelector_Selected(HorizontalSelector sender, int index, HorizontalSelectorButton button)
        {
            RefreshHeader(index);
            Pivot.SelectedIndex = index;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MultiAppBar.CurrentIndex = Pivot.SelectedIndex;
            Selector.Select(Pivot.SelectedIndex);
        }
        
        private void SettingsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var settings = new SettingsView();
            IoC.MainPage.ShowFlyout(settings);
        }

        private void NewsTypeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
