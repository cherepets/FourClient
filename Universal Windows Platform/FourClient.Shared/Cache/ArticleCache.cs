using System;
using FourToolkit.Esent;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace FourClient.Cache
{
    public delegate void CollectionStateChangedHandler(Article sender);

    public interface IArticleCache
    {
        ObservableCollection<Article> GetCollection();
        List<Article> GetHistory();
        Article FindInCache(string prefix, string link);
        Article FindInCollection(string prefix, string link);
        bool UpdateCollectionState(Article article);
        void Put(Article article);
        void RemoveOldEntites();
    }

    public class ArticleCache : Esent, IArticleCache
    {
        private const string Table = "ArticleCache";

        private ObservableCollection<Article> _collection;

        public ObservableCollection<Article> GetCollection()
        {
            if (_collection == null)
            _collection = Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    var rows = table.Where(r => r["InCollection"].AsBool);
                    var articles = rows?.Select(r => EsentSerializer.Deserialize<Article>(r)).ToList() ?? new List<Article>();
                    return new ObservableCollection<Article>(articles);
                }
            }) as ObservableCollection<Article>;
            return _collection;
        }

        public List<Article> GetHistory()
        {
            return Query(db =>
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        var rows = table.Where(r => !r["InCollection"].AsBool);
                        var articles = rows?.Select(r => EsentSerializer.Deserialize<Article>(r)).ToList() ?? new List<Article>();
                        return articles.OrderByDescending(a => a.CreatedOn).ToList();
                    }
                }) as List<Article>;
        }

        public Article FindInCache(string prefix, string link)
        {
            return Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    var rows = table.Where(r => r["Prefix"].AsString == prefix && r["Link"].AsString == link);
                    var row = rows?.FirstOrDefault();
                    if (row == null) return null;
                    return EsentSerializer.Deserialize<Article>(row);
                }
            }) as Article;
        }
        
        public Article FindInCollection(string prefix, string link)
            => GetCollection()?.FirstOrDefault(a => a.Prefix == prefix && a.Link == link);

        public bool UpdateCollectionState(Article article)
        {
            return (bool)Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    var updated = table.Update(r => r["Prefix"].AsString == article.Prefix && r["Link"].AsString == article.Link, "InCollection", article.InCollection) > 0;
                    if (updated && GetCollection() != null)
                    {
                        if (article.InCollection && !GetCollection().Contains(article))
                            GetCollection().Add(article);
                        if (!article.InCollection && GetCollection().Contains(article))
                            GetCollection().Remove(article);
                    }
                    return updated;
                }
            });
        }

        public void Put(Article article)
        {
            Query(db =>
            {
                var existent = FindInCollection(article.Prefix, article.Link);
                if (existent != null) return;
                using (var table = db.Tables[Table].Open())
                {
                    EsentCell[] cells = EsentSerializer.Serialize(article, table);
                    table.Insert(cells);
                }
                if (article.InCollection) GetCollection()?.Add(article);
            });
        }

        public void RemoveOldEntites()
        {
            Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    table.Delete(r => r["InCollection"].AsBool 
                    && (r["CreatedOn"].AsDateTime - DateTime.Now).TotalDays > Settings.Current.CacheDays);
                }
            });
        }
    }
}
