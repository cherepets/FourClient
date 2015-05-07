using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class UPG_GetPage : ITransformation
{
    const String Url = "http://www.upweek.ru/";
        
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
        //Генерация url
        urlBuilder
            .Append(Url)
            .Append("page/")
            .Append(page); 
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
        //Парсинг 
        var divs = xdoc.DescendantsByLocalName("div");        
        var filtered = divs.Where(d => d
            .AttributeValue("class") == "articles_bl")
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
            var h2 = xdiv.DescendantByLocalName("h2");
            var a = h2.DescendantByLocalName("a");
            var title = a.Value;
            var link = a.AttributeValue("href").Split('/').Where(l => l.Length > 0).Last();
            if (title.Length > Const.MaxLenghtLong)
                title = title.Substring(0, Const.MaxLenghtLong) + "...";
            if (title == null || link == null) return null;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", String.Empty},
                {"FullLink", Url + link},
                {"CommentLink", Url + link},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "UPG_GetPage/ParseDiv");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}