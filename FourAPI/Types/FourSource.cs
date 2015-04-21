using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace FourAPI.Types
{
    /// <summary>
    /// Source such as site (4PDA, Habrahabr, etc.)
    /// </summary>
    public class FourSource
    {
        /// <summary>
        /// List of sources selected by user
        /// </summary>
        public string MySources { get; set; }
        /// <summary>
        /// Source name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Prefix used as ID in FourAPI such as PDA, HBR, etc.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Source thumbnail url
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// Encoded list of types
        /// </summary>
        public string Base64Types
        {
            get { return _base64types; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _base64types = value;
                    var bytes = Convert.FromBase64String(_base64types);
                    var decoded = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    var xml = XDocument.Parse(decoded);
                    var pairs = xml.Descendants().Where(d => d.Name.LocalName == "Pair");
                    NewsTypes = new Dictionary<string, string>();
                    foreach (var pair in pairs)
                    {
                        NewsTypes.Add(
                            pair.Attribute("key").Value,
                            pair.Attribute("value").Value);
                        Base64Types = String.Empty;
                    }
                }
            }
        }
        /// <summary>
        /// Is source enabled
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Non-parsed Enabled field
        /// </summary>
        public string Availability
        {
            get { return Enabled.ToString(); }
            set
            {
                Enabled = value == "true";
            }
        }
        /// <summary>
        /// Is search available
        /// </summary>
        public bool Searchable { get; set; }
        /// <summary>
        /// Non-parsed Searchable field
        /// </summary>
        public string Searchability
        {
            get { return Searchable.ToString(); }
            set
            {
                Searchable = value == "true";
            }
        }
        /// <summary>
        /// News types
        /// </summary>
        public Dictionary<string, string> NewsTypes { get; set; }

        private string _base64types;
        
        /// <summary>
        /// Request an article
        /// </summary>
        /// <param name="link">Article address</param>
        /// <returns>Preprocessed web page</returns>
        public async Task<FourArticle> GetArticleAsync(string link)
        {
            return await GetArticleAsync(Prefix, link);
        }

        /// <summary>
        /// Get collection of sources
        /// </summary>
        /// <param name="cachePath">Where to save cache</param>
        /// <returns>Collection of sources</returns>
        public static async Task<ObservableCollection<FourSource>> GetSourcesAsync(string cachePath = null)
        {
            var client = new WebXslt.Client(Settings.Url);
            var collection = await client.CallAsync<FourSource>("SRC_GetList");
            // Save cache
            if (cachePath != null)
            {
                var xroot = new XElement("Collection");
                var xdoc = new XDocument(xroot);
                foreach (var source in collection)
                {
                    var xsource =
                        new XElement("Source",
                            new XElement("Name", source.Name),
                            new XElement("Prefix", source.Prefix),
                            new XElement("ImageUrl", source.ImageUrl),
                            new XElement("Base64Types", source.Base64Types),
                            new XElement("Availability", source.Availability.ToLower()),
                            new XElement("Searchability", source.Searchability.ToLower()));
                    xroot.Add(xsource);
                };
                var appData = ApplicationData.Current.LocalFolder;
                var fileCreate = appData.CreateFileAsync(cachePath, CreationCollisionOption.ReplaceExisting);
                var file = await fileCreate;
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    xdoc.Save(stream);
                }
            }
            return collection;
        }

        /// <summary>
        /// Request an article
        /// </summary>
        /// <param name="prefix">Prefix of the source</param>
        /// <param name="link">Article address</param>
        /// <returns>Preprocessed web page</returns>
        public static async Task<FourArticle> GetArticleAsync(string prefix, string link)
        {
            var client = new WebXslt.Client(Settings.Url);
            var collection = await client.CallAsync<FourArticle>(prefix + "_GetArticle", new string[] { link });
            var article = collection.First();
            var bytes = Convert.FromBase64String(article.HTML);
            article.HTML = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return article;
        }

        /// <summary>
        /// Get articles of selected type and page
        /// </summary>
        /// <param name="newsType">News type</param>
        /// <param name="pageNumber">Number of the page</param>
        /// <returns>Collection of articles</returns>
        public async Task<ObservableCollection<FourItem>> GetPageAsync(string newsType, int pageNumber)
        {
            var client = new WebXslt.Client(Settings.Url);
            var selectedType = NewsTypes[newsType] == "?" ? MySources : NewsTypes[newsType];
            var collection = await client.CallAsync<FourItem>(Prefix + "_GetPage", new string[] 
            {
                selectedType,
                pageNumber.ToString()
            });
            return collection;
        }

        /// <summary>
        /// Get articles from search results
        /// </summary>
        /// <param name="searchQuery">Search query</param>
        /// <param name="pageNumber">Number of the page</param>
        /// <returns>Collection of articles</returns>
        public async Task<ObservableCollection<FourItem>> SearchPageAsync(string searchQuery, int pageNumber)
        {
            var client = new WebXslt.Client(Settings.Url);
            var collection = await client.CallAsync<FourItem>(Prefix + "_GetPage", new string[] 
            {
                "search",
                pageNumber.ToString(),
                searchQuery
            });
            return collection;
        }

        public override string ToString()
        {
            return String.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n{7}", 
                Name, 
                Prefix, 
                ImageUrl, 
                Base64Types, 
                Enabled, 
                Availability, 
                Searchable, 
                Searchability);
        }
    }
}
