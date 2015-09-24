using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace FourClient.HtmlRender
{
    public class Html2Xaml : IHtmlRender
    {
        public event EventHandler Completed;

        private RichTextBlock _block;
        private string _html;

        public Html2Xaml()
        {
            _block = new RichTextBlock();
            _block.Margin = new Thickness(8, 0, 16, 32);
        }

        public FrameworkElement Implementation => new ScrollViewer { Content = _block };

        public Color Background { get; set; }

        public Color Foreground
        {
            get
            {
                return (_block.Foreground as SolidColorBrush).Color;
            }
            set
            {
                _block.Foreground = new SolidColorBrush(value);
            }
        }

        public int FontSize
        {
            get
            {
                return (int)(_block.FontSize / 6);
            }
            set
            {
                _block.FontSize = value * 6;
            }
        }

        public string FontFamily
        {
            get
            {
                return _block.FontFamily.Source;
            }
            set
            {
                _block.FontFamily = new FontFamily(value);
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
                _block.SetValue(global::Html2Xaml.Properties.HtmlProperty, html);
                _block.Blocks.Add(new Paragraph());
                _block.Blocks.Add(new Paragraph());
                Completed?.Invoke(this, null);
            }
        }
    }
}
