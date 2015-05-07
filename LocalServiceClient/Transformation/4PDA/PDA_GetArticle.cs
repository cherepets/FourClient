﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;

public class PDA_GetArticle : ITransformation
{        
    const String Url = "http://4pda.ru";
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
                    xdoc = http.GetXDocument(Url + args[0], true);   
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
                d.AttributeValue("class") == "content-box");
            Normalizer.FullCleanup(content);
            Normalizer.NormalizeMltbl(content);
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
            exceptionBuilder.AppendLine("Method: " + "PDA_GetArticle");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Data: " + data);
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}