using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

public class GKT_GetArticle : ITransformation
{
    const String YqlTemplate = "http://query.yahooapis.com/v1/public/yql" + 
        "?q=select%20*%20from%20html%20where%20url%3D'http%3A%2F%2Fgeekt" + 
        "imes.ru%2Fpost%2F{0}'%20and%20xpath%3D'.%2F%2Fdiv%5B%40class%20" +
        "%3D%20%22content%20html_format%22%5D'&format=xml";
        
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var data = String.Empty;
        try
        {
            var debug = args.Contains("debug");
            var retry = 0;
            XDocument xdoc;
            while (true)
            {
                try
                {
                    xdoc = XDocument.Load(String.Format(YqlTemplate, args[0].Urlize()));
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
            exceptionBuilder.AppendLine("Method: " + "GKT_GetArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Data: " + data);
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}