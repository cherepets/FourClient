using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

public class SRC_GetList : ITransformation
{
    const String Url = "http://cherrywebxslt.azurewebsites.net/App_Resources/src.xml";	
    public IEnumerable<Dictionary<string, string>> Transform(string[] args)
    {
        try
        {
    		var xml = XDocument.Load(Url);
            var sources = xml.DescendantsByLocalName("Source");
            var result = new List<Dictionary<string, string>>();
            foreach (var source in sources)
            {
                var fields = source.DescendantsByLocalName("Field");
                var dict = fields.ToDictionary(
                    f => f.AttributeValue("key"),  
                    f => f.AttributeValue("value"));
                var pairs = source.DescendantsByLocalName("Pair");
                var xdoc = new XDocument();
                var xlist = new XElement("List");
                foreach (var pair in pairs)
                {
                    var xelement = 
                        new XElement("Pair",
                            new XAttribute("key", pair.AttributeValue("key")),                            
                            new XAttribute("value", pair.AttributeValue("value")));
                    xlist.Add(xelement);
                }
                xdoc.Add(xlist);
                var bytes = Encoding.UTF8.GetBytes(xdoc.ToString());
                var base64 = Convert.ToBase64String(bytes);
                dict.Add("Base64Types", base64);
                result.Add(dict);
            }
            return result;
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "SRC_GetList");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            throw new Exception(exceptionBuilder.ToString());
        }
    }
}