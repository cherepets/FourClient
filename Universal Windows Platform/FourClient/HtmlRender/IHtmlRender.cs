using System;
using Windows.UI;
using Windows.UI.Xaml;

namespace FourClient.HtmlRender
{
    public interface IHtmlRender
    {
        event EventHandler Completed;
        
        FrameworkElement Implementation { get; }
        Color Background { get; set; }
        Color Foreground { get; set; }
        int FontSize { get; set; }
        string FontFamily { get; set; }
        bool CanGoBack { get; }
        string Html { get; set; }
    }
}
