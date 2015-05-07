using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class MZG_GetPage : ITransformation
{
    const String Url = "http://megamozg.ru/";
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
                    .Append("page")
                    .Append(page);
                break;
            case "search":
                urlBuilder
                    .Append("search/")
                    .Append("page")
                    .Append(page)
                    .Append("/?q=")
                    .Append(query)
					.Append("&target_type=posts&order_by=relevance");
                break;
            default:
                urlBuilder
					.Append("hub/")
                    .Append(newsType)
                    .Append("/")
                    .Append("page")
                    .Append(page);   
                break;
        }
        var retry = 0;
        XDocument xdoc;
        while (true)
        {
            try
            {
                //Запрос разный для разных страниц
                switch (newsType)
                {
                    case "search":
                        xdoc = urlBuilder.Proxify().Urlize().SelectFromYQL();
                        break;
                    default:
                        xdoc = urlBuilder.ToString().SelectFromYQL();
                        break;
                }
                break;
            }
            catch
            {
                retry++;
                if (retry >= Const.MaxRetry)
                    throw;
            }
        }
        Normalizer.NormalizeImages(xdoc);
        var divs = xdoc.DescendantsByLocalName("div");        
        var filtered = divs.Where(d => d
            .AttributeValue("class") == "post shortcuts_item")
            .ToList();      
        var list = filtered
            .Select(d => ParseDiv(d))
            .ToList();
        return list;    
    }
    
    private Dictionary<string, string> ParseDiv(XElement xdiv)
    {
        try
        {
            var a = xdiv.DescendantByLocalName("a");
            var title = a.Value
                .Replace("  ", String.Empty)
                .Replace("\r", String.Empty)
                .Replace("\n", String.Empty);
            var link = a.AttributeValue("href").Split('/').Where(l => l.Length > 0).Last();
            var image = xdiv.DescendantByLocalName("img").AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            if (image != null && image.StartsWith("/")) image = Url + image;
    
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://m.megamozg.ru/post/" + link},
                {"CommentLink", "http://m.megamozg.ru/post/" + link + "/comments"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "MZG_GetPage/ParseDiv");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}