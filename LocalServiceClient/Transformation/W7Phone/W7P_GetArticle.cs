using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

public class W7P_GetArticle : ITransformation
{
    const String YqlTemplate = "http://query.yahooapis.com/v1/public/yql" +
        "?q=select%20*%20from%20html%20where%20url%3D'http%3A%2F%2Fw7phon" +
        "e.ru%2F{0}%2F'%20and%20xpath%3D'.%2F%2Fdiv%5B%40class%20%3D%20%2" +
        "2entry-content%22%5D'&format=xml";
        
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var data = String.Empty;
        try
        {
            var debug = args.Contains("debug");
            var articleID = args[0].Replace("/", "%2F");
            
            var retry = 0;
            XDocument xdoc;
            while (true)
            {
                try
                {
                    xdoc = XDocument.Load(String.Format(YqlTemplate, articleID));
                    break;
                }
                catch
                {
                    retry++;
                    if (retry >= Const.MaxRetry)
                        throw;
                }
            }
            data = xdoc.ToString();
            var divs = xdoc.DescendantsByLocalName("div");     
            var content = divs.First();
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
            exceptionBuilder.AppendLine("Method: " + "HBR_GetArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Data: " + data);
            throw new Exception(exceptionBuilder.ToString());
        }
    }

}