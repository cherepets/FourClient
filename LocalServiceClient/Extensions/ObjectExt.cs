using System.IO;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Text;

public static class ObjectExt
{
    public static string Proxify(this object obj)
    {
        var bytes = Encoding.UTF8.GetBytes(obj.ToString());
        var base64 = Convert.ToBase64String(bytes);
        var proxyfied = "http://cherrywebxslt.azurewebsites.net/ProxyPage.aspx?base64url=" + base64;
        return proxyfied;
    }
}