using CenterCLR.Sgml;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

public static class StreamExt
{
    public static XDocument ToXDocument(this Stream stream)
    {       
        try
        {
            var xdoc = SgmlReader.Parse(stream);
            return xdoc;
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Inner Exception: " + ex.Message);
            throw new Exception(exceptionBuilder.ToString());
            throw;
        }
    }
}