using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebServiceClient
{
    /// <summary>
    /// Client for web xslt service
    /// </summary>
    public class Client
    {
        
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
        public async Task<List<T>> CallAsync<T>(string method, string[] args = null) where T : new()
        {
            var list = new List<T>();
            var plist = await GetPlaneObjects(method, args);
            foreach (var pclass in plist)
            {
                var t = new T();
                var type = t.GetType();
                var tprops = type.GetRuntimeProperties();
                foreach (var prop in pclass)
                {
                    var boxed = t as object;
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
            var url = string.Format("{0}?method={1}", _url, WebUtility.UrlEncode(method));
            if (args != null)
                for (int i = 0; i < args.Length; i++)
                {
                    url += string.Format("&a{0}={1}", i + 1, WebUtility.UrlEncode(args[i]));
                }
            var http = new HttpClient();
            try
            {
                var response = await http.GetStringAsync(url);
                if (string.IsNullOrEmpty(response)) throw new ConnectionException();
                return ParseContent(response);
            }
            catch (TimeoutException)
            {
                throw new ConnectionException();
            }
        }

        private List<Dictionary<string, string>> ParseContent(string content)
        {
            var xdoc = XDocument.Parse(content);
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
    }
}
