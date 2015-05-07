using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Text;

public static class HttpClientExt
{    
    public static XDocument GetXDocument(this HttpClient httpClient, String Url, bool win1251 = false)
    {
        try
        {
            var task = win1251 ? GetXDocumentWin1251Async(httpClient, Url) : GetXDocumentAsync(httpClient, Url);
            task.Wait();
            var xdoc = task.Result;
            return xdoc;
        }
        catch (AggregateException ae)
        {
            var ex = ae.InnerException;
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "HttpClient.GetXDocument");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Url: " + Url);
            throw new Exception(exceptionBuilder.ToString());
        }
        catch (Exception ex)
        {
            var exceptionBuilder = new StringBuilder();
            exceptionBuilder.AppendLine("Method: " + "HttpClient.GetXDocument");
            exceptionBuilder.AppendLine("Exception: " + ex.Message);
            exceptionBuilder.AppendLine("Stack: " + ex.StackTrace.ToString());
            exceptionBuilder.AppendLine("Url: " + Url);
            throw new Exception(exceptionBuilder.ToString());
        }
    }
    
    public static string GetString(this HttpClient httpClient, String Url)
    {
        var task = httpClient.GetStringAsync(Url);
        task.Wait();
        var result = task.Result;
        return result;
    }

    async private static Task<XDocument> GetXDocumentAsync(HttpClient httpClient, String Url)
    {
        var responce = await httpClient.GetStreamAsync(Url).ConfigureAwait(false);
        return responce.ToXDocument();
    }
    
    async private static Task<XDocument> GetXDocumentWin1251Async(HttpClient httpClient, String Url)
    {
        var responce = await httpClient.GetStreamAsync(Url).ConfigureAwait(false);
		var responceReader = new StreamReader(responce, Encoding.GetEncoding("windows-1251"));
        string responceText = responceReader.ReadToEnd();
		var correctedStream = responceText.ToStream();
        return correctedStream.ToXDocument();
    }
    
}