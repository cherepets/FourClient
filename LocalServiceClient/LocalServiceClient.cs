using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebServiceClient;

namespace LocalServiceClient
{
    public class LocalServiceClient
    {
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
            var task = new Task<string>(() => CallWithParameters(method, args));
            var response = await task;
            var plist = ParseLocalResponse(response);
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
        
        private string CallWithParameters(string method, string[] args)
        {
            try
            {
                var transformation = Transformations[method];
                var data = transformation.Transform(args);
                var xdoc = new XDocument();
                var xlist = new XElement("List");
                foreach (var planeObject in data)
                {
                    if (planeObject != null)
                    {
                        var xobj = new XElement("Object");
                        foreach (var item in planeObject)
                        {
                            xobj.Add(new XElement(item.Key, item.Value));
                        }
                        xlist.Add(xobj);
                    }
                }
                xdoc.Add(xlist);
                return xdoc.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex)
            {
                return String.Format("<Error>{0}</Error>", ex.Message);
            }
        }


        private List<Dictionary<string, string>> ParseLocalResponse(string localResponse)
        {
            var xdoc = XDocument.Parse(localResponse);
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

        public bool Implements(string method)
        {
            return Transformations.ContainsKey(method);
        }

        private readonly static Dictionary<string, ITransformation> Transformations = new Dictionary<string, ITransformation>
        {
            {"PDA_GetPage", new PDA_GetPage()},
            {"PDA_GetArticle", new PDA_GetArticle()},
            //{"W7P_GetPage", new W7P_GetPage()},
            //{"W7P_GetArticle", new W7P_GetArticle()},
            //{"HBR_GetPage", new HBR_GetPage()},
            //{"HBR_GetArticle", new HBR_GetArticle()},
            //{"GKT_GetPage", new GKT_GetPage()},
            //{"GKT_GetArticle", new GKT_GetArticle()},
            //{"MSE_GetPage", new MSE_GetPage()},
            //{"MSE_GetArticle", new MSE_GetArticle()},
            //{"WPS_GetPage", new WPS_GetPage()},
            //{"WPS_GetArticle", new WPS_GetArticle()},
            //{"IGD_GetPage", new IGD_GetPage()},
            //{"IGD_GetArticle", new IGD_GetArticle()},
            //{"ANK_GetPage", new ANK_GetPage()},
            //{"ANK_GetArticle", new ANK_GetArticle()},
            //{"WPF_GetPage", new WPF_GetPage()},
            //{"WPF_GetArticle", new WPF_GetArticle()},
            //{"FVK_GetPage", new FVK_GetPage()},
            //{"FVK_GetArticle", new FVK_GetArticle()},
            //{"MZG_GetPage", new MZG_GetPage()},
            //{"MZG_GetArticle", new MZG_GetArticle()},
            //{"UPG_GetPage", new UPG_GetPage()},
            //{"UPG_GetArticle", new UPG_GetArticle()},
        };
    }
}
