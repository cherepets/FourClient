using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

public class NEW_GetArticle : ITransformation
{
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        var parsedArgs = args[0].Split(';');
        var transformation = TransformationService.Transformations[parsedArgs[0] + "_GetArticle"];
        var argsList = new List<string> { parsedArgs[1] };
        if (args.Count() > 1 && args[1] == "debug") argsList.Add("debug");
        return transformation.Transform(argsList.ToArray());    
    }
}