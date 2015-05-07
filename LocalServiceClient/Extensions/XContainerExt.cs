using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public static class XContainerExt
{
    public static List<XElement> DescendantsByLocalName(this XContainer xElement, string name)
    {
        return xElement.Descendants().Where(e => e.Name.LocalName == name).ToList();
    }
    
    public static XElement DescendantByLocalName(this XContainer xElement, string name)
    {
        return xElement.Descendants().FirstOrDefault(e => e.Name.LocalName == name);
    }
    
    public static string AttributeValue(this XElement xElement, XName xname)
    {
        if (xElement == null) return null;
        var attribute = xElement.Attribute(xname);
        return attribute == null ? null : attribute.Value;
    }
}