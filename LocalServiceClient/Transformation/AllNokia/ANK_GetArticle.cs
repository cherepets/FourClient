using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;

public class ANK_GetArticle : ITransformation
{        
    const String Url = "http://allnokia.ru/news/";
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var data = String.Empty;
        try
        {
            var page = args[0].Replace("/", String.Empty);
            var debug = args.Contains("debug");
            var http = new HttpClient();
            var retry = 0;
            XDocument xdoc;
            while (true)
            {
                try
                {
                    xdoc = http.GetXDocument(Url + page, true);   
                    break;
                }
                catch
                {
                    retry++;
                    if (retry >= Const.MaxRetry)
                        throw;
                }
            } 
            var divs = xdoc.DescendantsByLocalName("div");        
            var content = divs.First(d => 
                d.AttributeValue("id") == "txt" + page);
            // Image path
            var images = content.DescendantsByLocalName("img");
            foreach (var img in images)
            {
                var src = img.AttributeValue("src");
                if (src.StartsWith("/"))
                {
                    src = "http://allnokia.ru/news/" + src;
                    img.Attribute("src").Value = src;
                }
            }
            // Image path
            Normalizer.FullCleanup(content);
            var builder = new StringBuilder();
            //Сборка ответа
            builder.AppendHtmlTop();
            builder.Append(content.ToString());
            builder.AppendHtmlBottom();
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var base64 = Convert.ToBase64String(bytes);
            var html = debug ? builder.ToString() : base64;
            return new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"HTML", html }
                }
            };
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "ANK_GetArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Data: " + data);
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}