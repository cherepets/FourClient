using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class ANK_GetPage : ITransformation
{
    const String Url = "http://allnokia.ru/news/";        
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var newsType = args[0].ToLowerInvariant();
        var page = args[1];
        var urlBuilder = new StringBuilder();
        urlBuilder.Append(Url);
        //Генерация url разная для разных страниц
        switch (newsType)
        {
            default:
            {
                urlBuilder
                    .Append("news_list.php?c=15")
                    .Append("&p=")
                    .Append(page);
                break;
            }
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
            default:
            {
                var divs = xdoc.DescendantsByLocalName("div")
                    .Where(d => d.AttributeValue("class") == "nblock");
                var list = divs
                    .Select(d => ParseDiv(d))
                    .ToList();
                return list;    
            }
        }
    }
    
    private Dictionary<string, string> ParseDiv(XElement xdiv)
    {
        try
        {
            var title = xdiv.DescendantByLocalName("div").Value;
            var a = xdiv.DescendantByLocalName("a");
            var link = a.AttributeValue("href");
            var img = xdiv.DescendantByLocalName("img");
            var image = img.AttributeValue("src");
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", "http://allnokia.ru/news/" + image},
                {"FullLink", "http://allnokia.ru/news/" + link},
                {"CommentLink", "http://allnokia.ru/news/" + link},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "ANK_GetPage/ParseDiv");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}