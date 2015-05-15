using FourAPI.Types;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace FourAPI
{
    /// <summary>
    /// Public API
    /// </summary>
    public static class Methods
    {
        public const string Url = "http://fourclientwebserver.azurewebsites.net/Service.aspx";
        /// <summary>
        /// Gets top articles
        /// </summary>
        /// <param name="cachePath">Where to save cache</param>
        /// <returns>Collection of articles</returns>
        public static async Task<ObservableCollection<FourItem>> GetTopAsync(string cachePath)
        {
            var client = new WebServiceClient.Client(Url);
            var collection = await client.CallAsync<FourItem>("MVW_GetPage");
            // Save cache
            if (cachePath != null)
            {
                var xroot = new XElement("Collection");
                var xdoc = new XDocument(xroot);
                foreach (var page in collection)
                {
                    var xsource =
                        new XElement("Top",
                            new XElement("CommentLink", page.CommentLink),
                            new XElement("FullLink", page.FullLink),
                            new XElement("Image", page.Image),
                            new XElement("Link", page.Link),
                            new XElement("Avatar", page.Avatar),
                            new XElement("Title", page.Title));
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
        /// Get collection of sources
        /// </summary>
        /// <param name="cachePath">Where to save cache</param>
        /// <returns>Collection of sources</returns>
        public static async Task<ObservableCollection<FourSource>> GetSourcesAsync(string cachePath = null)
        {
            var client = new WebServiceClient.Client(Url);
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
            var client = new WebServiceClient.Client(Url);
            var collection = await client.CallAsync<FourArticle>(prefix + "_GetArticle", new string[] { link });
            var article = collection.First();
            var bytes = Convert.FromBase64String(article.HTML);
            article.HTML = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return article;
        }

        /// <summary>
        /// Get articles of selected type and page
        /// </summary>
        /// <param name="source">News type</param>
        /// <param name="newsType">News type</param>
        /// <param name="pageNumber">Number of the page</param>
        /// <returns>Collection of articles</returns>
        public static async Task<ObservableCollection<FourItem>> GetItemsAsync(FourSource source, string newsType, int pageNumber)
        {
            var client = new WebServiceClient.Client(Url);
            var selectedType = source.NewsTypes[newsType] == "?" ? source.MySources : source.NewsTypes[newsType];
            var collection = await client.CallAsync<FourItem>(source.Prefix + "_GetPage", new string[] 
            {
                selectedType,
                pageNumber.ToString()
            });
            return collection;
        }

        /// <summary>
        /// Get articles from search results
        /// </summary>
        /// <param name="prefix">Source prefix</param>
        /// <param name="searchQuery">Search query</param>
        /// <param name="pageNumber">Number of the page</param>
        /// <returns>Collection of articles</returns>
        public static async Task<ObservableCollection<FourItem>> SearchPageAsync(string prefix, string searchQuery, int pageNumber)
        {
            var client = new WebServiceClient.Client(Url);
            var collection = await client.CallAsync<FourItem>(prefix + "_GetPage", new string[] 
            {
                "search",
                pageNumber.ToString(),
                searchQuery
            });
            return collection;
        }
    }
}
