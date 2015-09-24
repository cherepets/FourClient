using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Html2Xaml;
using System.Collections.Generic;
using System.Text;

namespace Html2Xaml
{
    /// <summary>
    /// Usage: 
    /// 1) In a XAML file, declare the above namespace, e.g.:
    ///    xmlns:h2xaml="using:Html2Xaml"
    ///     
    /// 2) In RichTextBlock controls, set or databind the Html property, e.g.:
	///    <RichTextBlock h2xaml:Properties.Html="{Binding ...}"/>
    ///    or
    ///    <RichTextBlock>
	///     <h2xaml:Properties.Html>
    ///         <![CDATA[
    ///             <p>This is a list:</p>
    ///             <ul>
    ///                 <li>Item 1</li>
    ///                 <li>Item 2</li>
    ///                 <li>Item 3</li>
    ///             </ul>
    ///         ]]>
	///     </h2xaml:Properties.Html>
    /// </RichTextBlock>
    /// </summary>
    public class Properties : DependencyObject
    {
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Properties), new PropertyMetadata(null, HtmlChanged));

        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

		public static Dictionary<string, Dictionary<string, string>> TagAttributes = null;
		private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is RichTextBlock)
			{
				string html = e.NewValue as string;

				// Get the target RichTextBlock
				RichTextBlock richText = d as RichTextBlock;
				if (richText == null) return;

				richText.Blocks.Clear();
				// Wrap the value of the Html property in a div and convert it to a new RichTextBlock
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<?xml version=\"1.0\"?>");
				sb.AppendLine("<RichTextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
				sb.AppendLine(Html2XamlConverter.Convert2Xaml(html, TagAttributes));
				sb.AppendLine("</RichTextBlock>");

				RichTextBlock newRichText = (RichTextBlock)XamlReader.Load(sb.ToString());

				// Move the blocks in the new RichTextBlock to the target RichTextBlock
				for (int i = newRichText.Blocks.Count - 1; i >= 0; i--)
				{
					Block b = newRichText.Blocks[i];
					newRichText.Blocks.RemoveAt(i);
					richText.Blocks.Insert(0, b);
				}
			}
		}
    }
}
