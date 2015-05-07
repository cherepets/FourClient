using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class IGD_GetPage : ITransformation
{
    const String Url = "http://www.iguides.ru/";        
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var newsType = args[0].ToLowerInvariant();
        var page = args[1];
        var offset = ((int.Parse(page) - 1) * 29 + 1).ToString();
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
                    .Append("index.php?")
                    .Append("PAGEN_4=")
                    .Append(page)
                    .Append("PAGEN_3=")
                    .Append(offset);
                break;
            case "search":
                urlBuilder
                    .Append("search/?q=")
                    .Append(query)
                    .Append("&PAGEN_1=")
                    .Append(page);
                break;
            default:
                urlBuilder
                    .Append(newsType)
                    .Append("/?PAGEN_1=")
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
            case "none":
            {
                var section = xdoc.DescendantsByLocalName("section")
                    .First(s => s.AttributeValue("class") == "right-column");  
                var articles = section.DescendantsByLocalName("article");        
                var filtered = articles.Where(a => a
                    .AttributeValue("class") == "news-box")
                    .ToList();      
                var list = filtered
                    .Select(a => ParseArticle(a))
                    .ToList();
                return list;    
            }    
            case "search":
            {
                var div = xdoc.DescendantsByLocalName("div")
                    .First(s => s.AttributeValue("class") == "search-page");  
                var articles = div.DescendantsByLocalName("div");        
                var filtered = articles.Where(a => a
                    .AttributeValue("class") == "main_news")
                    .ToList();    
                var list = filtered
                    .Select(d => ParseDiv(d))
                    .ToList();
                return list;    
            }
            default:
            {
                var div = xdoc.DescendantsByLocalName("div")
                    .First(s => s.AttributeValue("class") == "news-list");  
                var articles = div.DescendantsByLocalName("div");        
                var filtered = articles.Where(a => a
                    .AttributeValue("class") == "main_news")
                    .ToList();      
                var list = filtered
                    .Select(d => ParseDiv(d))
                    .ToList();
                return list;    
            }
        }
    }
    
    private Dictionary<string, string> ParseArticle(XElement xarticle)
    {
        try
        {
            var alist = xarticle.DescendantsByLocalName("a");
            var a = xarticle
                .DescendantsByLocalName("a")
                .First(t => t.AttributeValue("title") == null && t.AttributeValue("class") == null);
            var link = a.AttributeValue("href");
            var img = a.DescendantByLocalName("img");
            var title = img.AttributeValue("title");
            var image = "http://iguides.ru" + img.AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            if (image != null && image.StartsWith("/")) image = Url + image;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://iguides.ru" + link},
                {"CommentLink", "http://iguides.ru" + link + "#itape"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "IGD_GetPage/ParseArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xarticle.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }
    
    private Dictionary<string, string> ParseDiv(XElement xdiv)
    {
        try
        {
            var title = xdiv.DescendantByLocalName("span").Value;
            var a = xdiv.DescendantByLocalName("a");
            var link = a.AttributeValue("href");
            var img = xdiv.DescendantByLocalName("img");
            var image = img.AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            if (image != null && image.StartsWith("/")) image = Url + image;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://iguides.ru" + link},
                {"CommentLink", "http://iguides.ru" + link + "#itape"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "IGD_GetPage/ParseDiv");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}