using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class MVW_GetPage : ITransformation
{
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        try
        {
            var result = CherryDB.CachedTop();
            var list = Preprocess(result);
            return list;    
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "MVW_GetPage");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }

    private List<Dictionary<string, string>> Preprocess(List<MostViewedRow> stats)
    {
        return stats
            .Select(s => new Dictionary<string,string>
                {
                    {"Title", 
                        s.Title.Length > Const.MaxLenghtShort ?
                        s.Title.Substring(0, Const.MaxLenghtShort) + "..." :
                        s.Title},
                    {"Link", s.Prefix + ";" + s.Link},
                    {"Avatar", "http://cherrywebxslt.azurewebsites.net/App_Resources/" 
                        + s.Prefix.ToLower()
                        + ".png"},
                    {"Image", s.Image},
                    {"FullLink", s.FullLink},
                    {"CommentLink", s.CommentLink},
                })
            .ToList();
    }
}