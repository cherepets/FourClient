using Windows.UI.Xaml.Controls;

namespace FourClient.UserControls
{
    public abstract class PageHeaderBase : UserControl
    {
        public abstract bool IsProgressRunning { get; set; }
        public abstract string InitialText { get; set; }
        public abstract void SetTitle(string title);
    }
}
