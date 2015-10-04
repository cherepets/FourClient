using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FourClient.HtmlRender
{
    public class EdgeHtml : IHtmlRender
    {
        public event EventHandler Completed;

        private WebView _webView;
        private string _html;

        public EdgeHtml()
        {
            _webView = new WebView();
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
