using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class WPS_GetPage : ITransformation
{
    const String Url = "http://wp-seven.ru/";
        
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var newsType = args[0].ToLowerInvariant();
        var page = args[1];
        var query = String.Empty;
        if (args.Length > 2) 
        {
            var tquery = new StringBuilder();
            foreach(var c in args[2])
            {
                if (Const.AllowedSymbols.Contains(c))
                    tquery.Append(c);
            }
            query = tquery.ToString();
        }
        var urlBuilder = new StringBuilder();
        urlBuilder.Append(Url);
        //Генерация url разная для разных страниц
        switch (newsType)
        {
            case "none":
                urlBuilder
                    .Append("stat-i/")
                    .Append("page/")
                    .Append(page);
                break;
            case "search":
                urlBuilder
                    .Append("page/")
                    .Append(page)
                    .Append("/?s=")
                    .Append(query);
                break;
            default:
                urlBuilder
                    .Append("stat-i/")
                    .Append(newsType)
                    .Append("/")
                    .Append("page/")
                    .Append(page);   
                break;
        }
        var http = new HttpClient();
        var retry = 0;
        XDocument xdoc;
        while (true)
        {
            try
            {
                xdoc = http.GetXDocument(urlBuilder.ToString());
                break;
            }
            catch
            {
                retry++;
                if (retry >= Const.MaxRetry)
                    throw;
            }
        }
        //Парсинг разный для разных страниц
        switch (newsType)
        {
            case "search":
            {
                var divs = xdoc.DescendantsByLocalName("div");   
                var filtered = divs.Where(d => 
                    d.AttributeValue("class") == "entry clearfix_wp")
                    .ToList();     
                var list = filtered
                    .Select(d => ParseArticle(d))
                    .ToList();
                return list;    
            }    
            default:
            {
                var divs = xdoc.DescendantsByLocalName("div");        
                var filtered = divs.Where(d => d
                    .AttributeValue("class") == "post post-soft clearfix_wp")
                    .ToList();      
                var list = filtered
                    .Select(d => ParseDiv(d))
                    .ToList();
                return list;  
            }
        }
    }
    
    private Dictionary<string, string> ParseArticle(XElement xdiv)
    {
        try
        {
            var a = xdiv.DescendantsByLocalName("a").First(l => l.AttributeValue("rel") == "bookmark");
            var img = xdiv.DescendantByLocalName("img");
            var title = a.AttributeValue("title");
            var link = a.AttributeValue("href").Split('/').Where(l => l.Length > 0).Last();
            var image = img.AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://wp-seven.ru/stat-i/novosti/" + link},
                {"CommentLink", "http://wp-seven.ru/stat-i/novosti/" + link + "#comments"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "W7P_GetPage/ParseArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }
    
    private Dictionary<string, string> ParseDiv(XElement xdiv)
    {
        try
        {
            var a = xdiv.DescendantByLocalName("a");
            var img = xdiv.DescendantByLocalName("img");
            var title = img.AttributeValue("title");
            var link = a.AttributeValue("href").Split('/').Where(l => l.Length > 0).Last();
            var image = img.AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://wp-seven.ru/stat-i/novosti/" + link},
                {"CommentLink", "http://wp-seven.ru/stat-i/novosti/" + link + "#comments"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "W7P_GetPage/ParseDiv");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}