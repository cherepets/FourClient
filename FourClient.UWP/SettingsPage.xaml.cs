﻿using FourClient.UserControls;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace FourClient
{
    public sealed partial class SettingsPage : Page
    {
        public PageHeaderBase PageHeader;

        public SettingsPage()
        {
            this.InitializeComponent();
            RebuildUI();
        }

        private void RebuildUI()
        {
            if (SettingsService.IsPhone)
            {
                PageHeader = new MobileHeader();
                PageHeaderGrid.Children.Clear();
                PageHeaderGrid.Children.Add(PageHeader);
                PageHeader.SetTitle("Настройки");
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;
            RequestedTheme = SettingsService.MainTheme;
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            this.Frame.GoBack();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingsPage_BackRequested;
        }
    }
}