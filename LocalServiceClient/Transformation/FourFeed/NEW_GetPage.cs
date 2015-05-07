using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class NEW_GetPage : ITransformation
{
    private const int top = 10;
    
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {        
        var newsType = args[0].ToLowerInvariant();
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
        var d = "'";
        switch (newsType)
        {
            case "none":
            case "?":
            {
                var page = int.Parse(args[1]);
                var offset = top * (page - 1);
                var result = CherryDB.NewPages(null, top, offset);
                var list = Preprocess(result);
                return list;  
                break;
            }
            case "search":
            {
                var page = int.Parse(args[1]);
                var offset = top * (page - 1);
                var result = CherryDB.SearchPages(query, top, offset);
                var list = Preprocess(result);
                return list;  
                break;     
            }           
            default:
            {
                var prefixList = newsType
                    .Split(';')
                    .Where(p => p.Length == 3 && p != "NEW")
                    .Select(p => d + p + d)
                    .ToList();
                var page = int.Parse(args[1]);
                var offset = top * (page - 1);
                var result = CherryDB.NewPages(prefixList, top, offset);
                var list = Preprocess(result);
                return list;  
                break;
            }
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