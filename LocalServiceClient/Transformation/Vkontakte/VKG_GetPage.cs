using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public abstract class VKG_GetPage : ITransformation
{
    public abstract String Url { get; }
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var newsType = args[0].ToLowerInvariant();
        var page = args[1];
        var urlBuilder = new StringBuilder();
        var offset = 0;
        switch (page)
        {
            case "1":
                offset = 0;
                break;
            case "2":
                offset = 5;
                break;
            default:
                offset = (int.Parse(page) - 2) * 10 + 5;
                break;
        }
        urlBuilder.Append(Url);
        urlBuilder
            .Append("?offset=")
            .Append(offset)
            .Append("&amp;own=1#posts");
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
        Normalizer.NormalizeImages(xdoc);
        var divs = xdoc.DescendantsByLocalName("div");        
        var filtered = divs.Where(d => d
            .AttributeValue("class") == "wall_item")
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
            //wi_info
            var wi_info = xdiv.DescendantsByLocalName("div")
                .FirstOrDefault(x => x.AttributeValue("class") == "wi_info");
            if (wi_info == null) return null;
            //wi_body
            var wi_body = xdiv.DescendantsByLocalName("div")
                .FirstOrDefault(x => x.AttributeValue("class") == "wi_body");
            if (wi_body == null) return null;
            //pi_text
            var pi_text = wi_body.DescendantsByLocalName("div")
                .FirstOrDefault(x => x.AttributeValue("class") == "pi_text");
            if (pi_text == null) return null;
            var a = wi_info.DescendantByLocalName("a");
            var link = a.AttributeValue("href").Split('/').Where(l => l.Length > 0).LastOrDefault();
            var image = wi_body.DescendantByLocalName("img").AttributeValue("src");
            var title = pi_text.Value;
            if (title.Length > Const.MaxLenghtShort)
                title = title.Substring(0, Const.MaxLenghtShort) + "...";
            if (title == null || link == null) return null;
            
            return new Dictionary<string, string>
            {
                {"Title", title},
                {"Link", link},
                {"Image", image},
                {"FullLink", "http://m.vk.com/" + link},
                {"CommentLink", "http://m.vk.com/" + link + "?post_add#post_add"},
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "VKG_GetPage/ParseDiv");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("XElement: " + xdiv.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}