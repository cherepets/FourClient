using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using Windows.System;

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
            ShowBars();
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
        
        private bool _uiUpdated = true;

        public async void ShowBars()
        {
            if (!_uiUpdated) return;
            _uiUpdated = false;
            RefreshHeader(Pivot.SelectedIndex);
            PayButton.Visibility = PayState.IsPaid ? Visibility.Collapsed : Visibility.Visible;
            await MultiAppBar.ShowAsync();
            _uiUpdated = true;
        }

        public async void HideBars()
        {
            if (!_uiUpdated) return;
            _uiUpdated = false;
            PivotHeader.Visibility = Visibility.Collapsed;
            PayButton.Visibility = Visibility.Collapsed;
            await MultiAppBar.HideAsync();
            _uiUpdated = true;
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
            if (IoC.FeedView.CurrentSource == null) return;
            var searchInput = new TextInput { VerticalAlignment = VerticalAlignment.Top, Placeholder = "поиск" };
            Flyout.ShowFlyout(searchInput);
            searchInput.SetFocus();
            searchInput.InputCanceled += () => Flyout.HideFlyout();
            searchInput.InputCompleted += input =>
            {
                var feed = IoC.FeedView.CurrentSource.SearchFeed[input];
                IoC.FeedView.SetItemsSource(feed);
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

        private void HistoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var history = new HistoryView();
            IoC.MainPage.ShowFlyout(history);
        }

        private void SettingsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var settings = new SettingsView();
            IoC.MainPage.ShowFlyout(settings);
        }

        private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e)
            => IoC.FeedView.Refresh();

        private void NewsTypeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var button = (Button)sender;
            var source = IoC.FeedView.CurrentSource;
            if (source != null)
            {
                var items = source.Topics.Select(q =>
                new ContextMenuItem(q.Key,
                    () =>
                    {
                        IoC.FeedView.SetItemsSource(source.TopicFeed[q.Value]);
                        button.Content = q.Key;
                    }));
                var menu = new ContextMenu(IoC.MainPage.Flyout, button, items.ToArray());
                menu.VerticalAlignment = VerticalAlignment.Bottom;
                var t = menu.Margin;
                menu.Margin = new Thickness(t.Left, 0, t.Right, button.ActualHeight);
                IoC.MainPage.ShowFlyout(menu);
            }
        }

        private async void PayButton_Tapped(object sender, TappedRoutedEventArgs e)
            => await new MessageDialog(
@"Платная версия необходима для оплаты облачного сервиса в Microsoft Azure, который разбирает инфорацию с сайтов, вырезает из статей лишнее, обновляет список источников отдельно от приложения и многое дургое.
Платная версия включает:
- Отсутствие этой кнопки :)
- Больше настроек
- Обновления выходят после тестирования на бесплатной версии",
"Купить платную версию?")
                .WithCommand("Купить", Buy)
                .WithCommand("Отмена")
                .SetDefaultCommandIndex(0)
                .SetCancelCommandIndex(1)
                .ShowAsync();

        private async void Buy()
            => await Launcher.LaunchUriAsync(new Uri("https://www.microsoft.com/store/apps/9nblggh0chck"));
    }
}
