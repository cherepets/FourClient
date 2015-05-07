using System.IO;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;

public static class StringExt
{
    const String YqlTemplate = "http://query.yahooapis.com/v1/public/yql" +
        "?q=select%20*%20from%20html%20where%20url%3D'{0}'";
    
    public static Stream ToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    
    public static List<Dictionary<string, string>> ToPlane(this string s)
    {
        return new List<Dictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                { "Value", s }
            }
        };
    }
    
    public static string Urlize(this string s)
    {
        return s
            .Replace("/", "%2F")
            .Replace(" ", "%20")
            .Replace("&", "%26")
            .Replace("?", "%3F")
            .Replace(":", "%3A");
    }
    
    public static XDocument SelectFromYQL(this string s)
    {
        var url = PreviewYQL(s);
        return XDocument.Load(String.Format(YqlTemplate, s.Urlize()));
    }
    
    public static string PreviewYQL(this string s)
    {
        return String.Format(YqlTemplate, s.Urlize());
    }
    
    public static StringBuilder AppendHtmlTop(this StringBuilder sb)
    {
        return sb.Append("<html><head>")
            .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no\" />")
            .Append("<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />")
            .Append("<meta name=\"format-detection\" content=\"telephone=yes\" />")
            .Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />")
            .Append("<meta name=\"MobileOptimized\" content=\"720\" />")
            .Append("<meta name=\"HandheldFriendly\" content=\"True\" />")
            .Append("<style type=\"text/css\">body { word-wrap: break-word; }</style>")
            .Append("</head><body bgcolor={0} text={1} link={1} alink={1} vlink={1}>")
            .Append("<font face=\"{3}\" size={2}>");
    }
    
    public static StringBuilder AppendHtmlBottom(this StringBuilder sb)
    {
        return sb.Append("</font><font face=\"Segoe UI\" size=4><br><br></font></body></html>");
    }
}