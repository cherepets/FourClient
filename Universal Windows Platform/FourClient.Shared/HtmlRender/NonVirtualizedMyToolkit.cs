using MyToolkit.Controls;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FourClient.HtmlRender
{
    public class NonVirtualizedMyToolkit : IHtmlRender
    {
        public event EventHandler Completed;

        private HtmlView _view;
        private string _html;

        public NonVirtualizedMyToolkit()
        {
            _view = new HtmlView();
            _view.Margin = new Thickness(8, 0, 16, 50);
        }

        public FrameworkElement Implementation => new ScrollViewer { Content = _view };

        public Color Background { get
            {
                return (_view.Background as SolidColorBrush).Color;
            }
            set
            {
                _view.Background = new SolidColorBrush(value);
            }
        }

        public Color Foreground
        {
            get
            {
                return (_view.Foreground as SolidColorBrush).Color;
            }
            set
            {
                _view.Foreground = new SolidColorBrush(value);
            }
        }

        public int FontSize
        {
            get
            {
                return (int)(_view.FontSize / 6);
            }
            set
            {
                _view.FontSize = value * 6;
            }
        }

        public string FontFamily
        {
            get
            {
                return _view.FontFamily.Source;
            }
            set
            {
                _view.FontFamily = new FontFamily(value);
            }
        }

        public bool CanGoBack => false;

        public string Html
        {
            get
            {
                return _html;
            }
            set
            {
                _html = value;
                var html = _html.Replace("body { word-wrap: break-word; text-align: left }", string.Empty);
                _view.Html = html;
                Completed?.Invoke(this, null);
            }
        }
    }
}
