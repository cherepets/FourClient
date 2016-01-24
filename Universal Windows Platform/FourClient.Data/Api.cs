using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourClient.Data
{
    public static class Api
    {
        public const string Url = "http://fourclientwebserver.azurewebsites.net/Service.aspx";

        public static ObservableCollection<FeedItem> GetTop()
        {
            var client = new WebServiceClient(Url);
            var task = client.CallAsync<FeedItem>("MVW_GetPage");
            return new CollectionWithCache<FeedItem>(IoC.TopCache, task);
        }
        
        public static ObservableCollection<Source> GetSources()
        {
            var client = new WebServiceClient(Url);
            var task = client.CallAsync<Source>("SRC_GetList");
            return new CollectionWithCache<Source>(IoC.SourceCache, task);
        }
        
        public static async Task<Article> GetArticleAsync(string prefix, string link)
        {
            var client = new WebServiceClient(Url);
            var collection = await client.CallAsync<Article>(prefix + "_GetArticle", new string[] { link });
            var article = collection.First();
            var bytes = Convert.FromBase64String(article.HTML);
            article.HTML = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return article;
        }
        
        public static async Task<List<FeedItem>> GetItemsAsync(Source source, string newsType, int pageNumber)
        {
            var client = new WebServiceClient(Url);
            var selectedType = source.NewsTypes[newsType] == "?" ? source.MySources : source.NewsTypes[newsType];
            var collection = await client.CallAsync<FeedItem>(source.Prefix + "_GetPage", new string[] 
            {
                selectedType,
                pageNumber.ToString()
            });
            return collection;
        }
        
        public static async Task<List<FeedItem>> SearchPageAsync(string prefix, string searchQuery, int pageNumber)
        {
            var client = new WebServiceClient(Url);
            var collection = await client.CallAsync<FeedItem>(prefix + "_GetPage", new string[] 
            {
                "search",
                pageNumber.ToString(),
                searchQuery
            });
            return collection;
        }
    }
}
