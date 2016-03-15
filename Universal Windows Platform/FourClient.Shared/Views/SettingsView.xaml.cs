using FourClient.Library;
using FourToolkit.Extensions.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace FourClient.Views
{
    public sealed partial class SettingsView
    {
        private List<ScrollViewer> Scrollers = new List<ScrollViewer>();

        public SettingsView()
        {
            InitializeComponent();
            Loaded += SettingsView_Loaded;
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var pair in StartUpTypeExt.GetDictionary())
            {
                var item = new ComboBoxItem
                {
                    Tag = pair.Key,
                    Content = pair.Value
                };
                StartUpBox.Items.Add(item);
            }
            foreach (var pair in TwoColumnsModeExt.GetDictionary())
            {
                var item = new ComboBoxItem
                {
                    Tag = pair.Key,
                    Content = pair.Value
                };
                TwoColumnsModeBox.Items.Add(item);
            }
            AppThemeToggle.IsOn = Settings.Current.AppTheme;
            ArticleThemeToggle.IsOn = Settings.Current.ArticleTheme;
            LiveTileBox.IsChecked = Settings.Current.LiveTile;
            ToastBox.IsChecked = Settings.Current.Toast;
            FilterInterestingBox.IsChecked = Settings.Current.FilterInteresting;
            AllowRotationToggle.IsOn = Settings.Current.AllowRotation;
            EnableFlipViewerToggle.IsOn = Settings.Current.EnableFlipViewer;
            SetComboBoxValue(StartUpBox, Settings.Current.ShowAtStartup.GetName());
            SetComboBoxValue(TwoColumnsModeBox, Settings.Current.TwoColumnsMode.GetName());
            SetComboBoxValue(YouTubeBox, Settings.Current.YouTube);
            SetComboBoxValue(AlignBox, Settings.Current.Align);
            SetComboBoxValue(FontBox, Settings.Current.FontFace);
            FontSlider.Value = Settings.Current.FontSize;
            FontSliderCap.Text = FontSlider.Value.ToString();
            ScrollEventSlider.Value = Settings.Current.ScrollEventThreshold;
            ScrollEventSliderCap.Text = ScrollEventSlider.Value.ToString();
            CacheSlider.Value = Settings.Current.CacheDays;
            CacheSliderCap.Text = CacheSlider.Value.ToString();
            VersionBlock.Text = $"v. {Settings.Current.DisplayVersion}";
            #region Free version restrictions
            if (!PayState.IsPaid)
            {
                ToastBox.IsEnabled = false;
                FilterInterestingBox.IsEnabled = false;
                EnableFlipViewerToggle.IsEnabled = false;
                FontBox.IsEnabled = false;
                YouTubeBox.IsEnabled = false;
            }
            #endregion
            _init = true;
        }

        private bool _init;

        private void AppThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.AppTheme = AppThemeToggle.IsOn;
        }

        private void ArticleThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.ArticleTheme = ArticleThemeToggle.IsOn;
        }

        private void LiveTileBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.LiveTile = LiveTileBox.IsChecked ?? true;
        }

        private void ToastBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.Toast = ToastBox.IsChecked ?? true;
        }

        private void FilterInterestingBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.FilterInteresting = FilterInterestingBox.IsChecked ?? true;
            IoC.InterestingView.Refresh();
        }

        private void AllowRotationToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.AllowRotation = AllowRotationToggle.IsOn;
        }

        private void EnableFlipViewerToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.EnableFlipViewer = EnableFlipViewerToggle.IsOn;
        }

        private void StartUpBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.ShowAtStartup = (StartUpType)GetComboBoxTag(StartUpBox);
        }

        private void TwoColumnsModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.TwoColumnsMode = (TwoColumnsMode)GetComboBoxTag(TwoColumnsModeBox);
        }

        private void YouTubeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.YouTube = GetComboBoxValue(YouTubeBox);
        }

        private void AlignBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.Align = GetComboBoxValue(AlignBox);
        }

        private void FontBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.FontFace = GetComboBoxValue(FontBox);
        }

        private void FontSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.FontSize = (int)FontSlider.Value;
            FontSliderCap.Text = FontSlider.Value.ToString();
        }

        private void CacheSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.CacheDays = (int)CacheSlider.Value;
            CacheSliderCap.Text = CacheSlider.Value.ToString();
        }

        private void ScrollEventSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_init) return;
            Settings.Current.ScrollEventThreshold = (int)ScrollEventSlider.Value;
            ScrollEventSliderCap.Text = ScrollEventSlider.Value.ToString();
        }

        private void SetComboBoxValue(ComboBox box, string value)
        {
            var item = box.Items.FirstOrDefault(c => ((ComboBoxItem)c).Content as string == value);
            if (item != null) box.SelectedValue = item;
        }

        private string GetComboBoxValue(ComboBox box)
        {
            return (((ComboBoxItem)box.SelectedItem).Content as string);
        }

        private object GetComboBoxTag(ComboBox box)
        {
            return (((ComboBoxItem)box.SelectedItem).Tag);
        }

        private async void ContactMe_Tapped(object sender, TappedRoutedEventArgs e)
            => await Launcher.LaunchUriAsync(new Uri("mailto:?to=cherepets@live.ru&subject=FourClient (отзыв)"));

        private async void RateMe_DesiredRatingSelected(object sender, int e)
            => await Launcher.LaunchUriAsync(new Uri($"ms-windows-store:review?ProductId={App.Current.GetProductId()}"));

        private async void RateMe_UndesiredRatingSelected(object sender, int e)
            => await Launcher.LaunchUriAsync(new Uri($"mailto:?to=cherepets@live.ru&subject=FourClient ({e} здезд)"));

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
            => Scrollers.Add(sender as ScrollViewer);

        private void SettingsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var isWide = e.NewSize.Width >= e.NewSize.Height;
            RootPanel.Orientation = isWide ? Orientation.Horizontal : Orientation.Vertical;
            RootScroller.HorizontalScrollMode = isWide ? ScrollMode.Enabled : ScrollMode.Disabled;
            RootScroller.HorizontalScrollBarVisibility = isWide ? ScrollBarVisibility.Visible : ScrollBarVisibility.Disabled;
            RootScroller.VerticalScrollMode = isWide ? ScrollMode.Disabled : ScrollMode.Enabled;
            RootScroller.VerticalScrollBarVisibility = isWide ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Visible;
            Scrollers.ForEach(s => s.VerticalScrollMode = isWide ? ScrollMode.Enabled : ScrollMode.Disabled);
        }

        private void RootScroller_SizeChanged(object sender, SizeChangedEventArgs e)
            => Scrollers.ForEach(s => s.MaxHeight = e.NewSize.Height);
    }
}
