using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebServiceClient
{
    /// <summary>
    /// Client for web xslt service
    /// </summary>
    public class Client
    {
        
        private static TimeSpan ConstTimeout = TimeSpan.FromSeconds(180);

        private string _url;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="Url">Serive url</param>
        public Client(string Url)
        {
            _url = Url;
        }

        /// <summary>
        /// Call the service
        /// </summary>
        /// <typeparam name="T">Deserialize result to list of objects of type T</typeparam>
        /// <param name="method">Method name</param>
        /// <param name="args">Method parameters</param>
        /// <returns>Collection of T</returns>
        public async Task<ObservableCollection<T>> CallAsync<T>(string method, string[] args = null) where T : new()
        {
            var list = new ObservableCollection<T>();
            var plist = await GetPlaneObjects(method, args);
            foreach (var pclass in plist)
            {
                var t = new T();
                var type = t.GetType();
                var tprops = type.GetRuntimeProperties();
                foreach (var prop in pclass)
                {
                    var boxed = t as Object;
                    var tprop = tprops.Where(p => p.Name == prop.Key && p.CanWrite);
                    if (tprop.Any()) tprop.First().SetValue(boxed, prop.Value);
                    t = (T)boxed;
                }
                list.Add(t);
            }
            return list;
        }

        private async Task<List<Dictionary<string, string>>> GetPlaneObjects(string method, string[] args = null)
        {
	        using (var http = new HttpClient { Timeout = ConstTimeout } )
	        {
	            var action = args == null ? "Call" : "CallWithParameters";
	            var argsXml = args == null ? String.Empty : BuildString(args);
	            var soapXml = String.Format(XML, action, method, argsXml);
	            http.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/" + action);
                var content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
                var url = new Uri(_url);
                using (var response = await http.PostAsync(url, content))
	            {
	                var soapResponse = await response.Content.ReadAsStringAsync();
                    if (String.IsNullOrEmpty(soapResponse))
                        throw new ConnectionException();
	                return ParseSoapResponse(soapResponse);
	            }
	        }
        }

        private List<Dictionary<string, string>> ParseSoapResponse(string soapResponse)
        {
            var xsoap = XDocument.Parse(soapResponse);
            var body = xsoap.Descendants().First(d => d.Name.LocalName.Contains("Result")).Value;
            var decoded = body
                .Replace("&lt;", "<")
                .Replace("&gt;", ">");
            var xdoc = XDocument.Parse(decoded);
            var error = xdoc.Descendants().Where(d => d.Name.LocalName == "Error");
            if (error.Any())
            {
                throw new ServiceException(error.First().Value);
            }
            var xobjects = xdoc.Descendants().Where(d => d.Name.LocalName == "Object");
            var plist = xobjects.Select(o => ParseXObject(o)).ToList();
            return plist;
        }

        private Dictionary<string, string> ParseXObject(XElement xobject)
        {
            var pclass = new Dictionary<string, string>();
            var props = xobject.Elements().ToDictionary(o => o.Name.LocalName, o => o.Value);
            foreach (var item in props)
            {
                pclass.Add(item.Key, item.Value);
            }
            return pclass;
        }

        private string BuildString(string[] args)
        {
            var builder = new StringBuilder();
            builder.Append("<args>");
            foreach (var a in args)
            {
                var formatted = String.Format("<string>{0}</string>", a);
                builder.Append(formatted);                
            }
            builder.Append("</args>");
            return builder.ToString();
        }

        private const string XML = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <{0} xmlns=""http://tempuri.org/"">
      <method>{1}</method>
      {2}
    </{0}>
  </soap:Body>
</soap:Envelope>";
    }
}
