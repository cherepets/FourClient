using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class PDA_GetPage : ITransformation
{
    const String Url = "http://4pda.ru/";
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
        query = HttpUtility.UrlEncode(query, Encoding.GetEncoding("windows-1251"));
        var urlBuilder = new StringBuilder();
        urlBuilder.Append(Url);
        //Генерация url разная для разных страниц
        switch (newsType)
        {
            case "none":
                urlBuilder
                    .Append("page/")
                    .Append(page)
                    .Append("?cache=")
                    .Append(Guid.NewGuid().ToString());
                break;
            case "search":
                urlBuilder
                    .Append("page/")
                    .Append(page)
                    .Append("/?s=")
                    .Append(query)
                    .Append("&cache=")
                    .Append(Guid.NewGuid().ToString());
                break;
            default:
                urlBuilder
                    .Append(newsType)
                    .Append("/")
                    .Append("page/")
                    .Append(page)
                    .Append("?cache=")
                    .Append(Guid.NewGuid().ToString());
                break;
        }
        var http = new HttpClient();
        var retry = 0;
        XDocument xdoc;
        while (true)
        {
            try
            {
                xdoc = http.GetXDocument(urlBuilder.ToString(), true);
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
                var filtered = divs.Where(d => d
                    .AttributeValue("class") == "photo")
                    .ToList();      
                var list = filtered
                    .Select(p => ParsePhoto(p))
                    .ToList();
                return list;       
            }    
            case "reviews":
            {
                var lis = xdoc.DescendantsByLocalName("li");        
                var filtered = lis.Where(d => d
                    .AttributeValue("itemtype") == "http://schema.org/Product")
                    .ToList();      
                var list = filtered
                    .Select(l => ParseLi(l))
                    .ToList();
                return list;       
            }    
            default:
            {
                var articles = xdoc.DescendantsByLocalName("article");
                var filtered = articles.Where(a => a
                    .Attribute("class")
                    .Value == "post")
                    .ToList();
                var list = filtered
                    .Select(a => ParseArticle(a))
                    .ToList();
                return list;     
            }       
        }
    }
    
    private Dictionary<string, string> ParsePhoto(XElement xphoto)
    {
        try
        {
            var a = xphoto
                .DescendantsByLocalName("a")
                .First(l => l.AttributeValue("title") != null);
            var title = a.AttributeValue("title");
            var link = a.AttributeValue("href");
            var image = xphoto.DescendantByLocalName("img").AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
    
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://4pda.ru" + link},
                {"CommentLink", "http://4pda.ru" + link + "#comments"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "PDA_GetPage/ParsePhoto");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xphoto.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }
    
    private Dictionary<string, string> ParseLi(XElement xli)
    {
        try
        {
            var links = xli.DescendantsByLocalName("a");
            var bookmark = links.First(l => 
                l.AttributeValue("rel") == "bookmark");
            var title = bookmark.AttributeValue("title");
            var link = bookmark.Attribute("href").Value;
            var image = xli.DescendantByLocalName("img").Attribute("src").Value;
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
    
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://4pda.ru" + link},
                {"CommentLink", "http://4pda.ru" + link + "#comments"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "PDA_GetPage/ParseLi");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xli.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }
    
    private Dictionary<string, string> ParseArticle(XElement xarticle)
    {
        try
        {
            var links = xarticle.DescendantsByLocalName("a");
            var bookmark = links.First(l => 
                l.AttributeValue("rel") == "bookmark");
            var title = bookmark.Value;
            var link = bookmark.Attribute("href").Value;
            var description = xarticle.DescendantByLocalName("p").Value;
            var image = xarticle.DescendantByLocalName("img").Attribute("src").Value;
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
    
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://4pda.ru" + link},
                {"CommentLink", "http://4pda.ru" + link + "#comments"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "PDA_GetPage/ParseArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xarticle.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}