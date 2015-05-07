using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;

public class VKG_GetArticle : ITransformation
{        
    const String Url = "http://m.vk.com/";
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var data = String.Empty;
        var wi_deletable = new List<string>
        {
            "wi_head", "wi_like_wrap", "wi_buttons"  
        };
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
                d.AttributeValue("class") == "wall_item single_wall_item bl_item");
            Normalizer.FullCleanup(content);
            var wis = content.DescendantsByLocalName("div")
                .Where(d => wi_deletable.Contains(d.AttributeValue("class")));
            foreach (var wi in wis)
            {
                wi.Remove();
            }
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
            exceptionBuilder.AppendLine("Method: " + "VKG_GetArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Data: " + data);
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}