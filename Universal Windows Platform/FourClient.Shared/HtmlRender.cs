using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FourClient
{
    public class HtmlRender
    {
        public event EventHandler Completed;
        public event EventHandler ScrollDown;
        public event EventHandler ScrollUp;

        private WebView _webView;
        private string _html;

        private DateTime _lastEventTime = DateTime.MinValue;

        public HtmlRender()
        {
            _webView = new WebView();
            _webView.ScriptNotify += _webView_ScriptNotify;
        }

        private void _webView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (DateTime.Now - _lastEventTime < TimeSpan.FromMilliseconds(600))
                return;
            _lastEventTime = DateTime.Now;
            if (e.Value == "scrollDown") ScrollDown?.Invoke(this, null);
            if (e.Value == "scrollUp") ScrollUp?.Invoke(this, null);
        }

        public FrameworkElement Implementation => _webView;

        public Color Background
        {
            get
            {
                return _webView.DefaultBackgroundColor;
            }
            set
            {
                _webView.DefaultBackgroundColor = value;
            }
        }

        public Color Foreground { get; set; }

        public int FontSize { get; set; }

        public string FontFamily { get; set; }

        public bool CanGoBack => _webView.CanGoBack;

        public string Html
        {
            get
            {
                return _html;
            }
            set
            {
                _html = value;
                _webView.NavigateToString(_html);
                _webView.NavigationCompleted += (s, e) => Completed?.Invoke(this, null);
            }
        }
    }
}
