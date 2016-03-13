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

        public static ObservableCollection<FeedItem> GetTop(bool withCache = true)
        {
            var client = new WebServiceClient(Url);
            var task = client.CallAsync<FeedItem>("MVW_GetPage");
            if (withCache)
                return new CollectionWithCache<FeedItem>(IoC.TopCache, task);
            return new ObservableCollection<FeedItem>(task.Result);
        }
        
        public static ObservableCollection<Source> GetSources()
        {
            var client = new WebServiceClient(Url);
            var task = client.CallAsync<Source>("SRC_GetList");
            return new CollectionWithCache<Source>(IoC.SourceCache, task);
        }
        
        public static async Task<string> GetArticleAsync(string prefix, string link)
        {
            var client = new WebServiceClient(Url);
            var collection = await client.CallAsync<Article>(prefix + "_GetArticle", new string[] { link });
            var article = collection.First();
            var bytes = Convert.FromBase64String(article.HTML);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        
        public static async Task<List<FeedItem>> GetItemsAsync(Source source, string topic, int pageNumber)
        {
            var client = new WebServiceClient(Url);
            var selectedTopic = topic;
            if (topic == "?") selectedTopic = string.Join(";", IoC.SourceSelector.Sources.Select(q => q.Prefix));
            var collection = await client.CallAsync<FeedItem>(source.Prefix + "_GetPage", new string[] 
            {
                selectedTopic,
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
