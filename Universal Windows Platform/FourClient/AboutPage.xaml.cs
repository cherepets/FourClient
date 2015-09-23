﻿using FourClient.Extensions;
using FourClient.UserControls;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FourClient
{
    public sealed partial class AboutPage : Page
    {
        public PageHeaderBase PageHeader;

        public AboutPage()
        {
            InitializeComponent();
            RebuildUI();
        }

        private void RebuildUI()
        {
            if (SettingsService.IsPhone)
            {
                PageHeader = new MobileHeader();
                PageHeaderGrid.Children.Clear();
                PageHeaderGrid.Children.Add(PageHeader);
                PageHeader.SetTitle("О приложении");
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += AboutPage_BackRequested;
            RequestedTheme = SettingsService.MainTheme;
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            View.Animate();
        }

        private void AboutPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= AboutPage_BackRequested;
            if (SettingsService.IsPhone)
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }
    }
}
