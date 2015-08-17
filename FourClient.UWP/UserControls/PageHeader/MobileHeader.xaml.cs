using FourClient.Extensions;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FourClient.UserControls
{
    public sealed partial class MobileHeader : PageHeaderBase
    {
        public override bool IsProgressRunning
        {
            get { return StatusProgressBar.Visibility == Visibility.Visible; }
            set { StatusProgressBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public override string InitialText
        {
            get { return StatusBox.Text; }
            set
            {
                if (value == null) return;
                StatusBox.Text = value;
            }
        }

        public MobileHeader()
        {
            this.InitializeComponent();
        }

        public override async void SetTitle(string title)
        {
            await HideStatusBoxBoard.PlayAsync();
            StatusBox.Text = title;
            await ShowStatusBoxBoard.PlayAsync();
        }

        private async void TimerStart()
        {
            while (true)
            {
                TimeBox.Text = DateTime.Now.ToString("H:mm");
                await Task.Delay(10000);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TimerStart();
        }

    }
}
