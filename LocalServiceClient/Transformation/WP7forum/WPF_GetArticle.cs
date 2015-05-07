using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;

public class WPF_GetArticle : ITransformation
{        
    const String Url = "http://wp7forum.ru/";
    
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var data = String.Empty;
        try
        {
            var debug = args.Contains("debug");
            var http = new HttpClient();
            var retry = 0;
            XDocument xdoc;
            while (true)
            {
                try
                {
                    xdoc = http.GetXDocument(Url + args[0]);   
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
                d.AttributeValue("itemprop") == "articleBody" ||
                d.AttributeValue("itemprop") == "text");
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
            exceptionBuilder.AppendLine("Method: " + "WPF_GetArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Data: " + data);
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}