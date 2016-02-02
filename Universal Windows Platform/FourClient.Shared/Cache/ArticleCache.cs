using System;
using FourToolkit.Esent;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Cache
{
    public interface IArticleCache
    {
        string FindHtml(string prefix, string link);
        List<Article> GetCollection();
        void Put(Article item);
        void RemoveOldEntites();
    }

    public class ArticleCache : Esent, IArticleCache
    {
        private const string Table = "ArticleCache";
        
        public List<Article> GetCollection()
        {
            return Query(db =>
            {
                try
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        var rows = table.Where(r => r["InCollection"].AsBool);
                        return rows?.Select(r => EsentSerializer.Deserialize<Article>(r)).ToList();
                    }
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                    return null;
                }
            }) as List<Article>;
        }

        public string FindHtml(string prefix, string link)
        {
            return Query(db =>
            {
                try
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        var rows = table.Where(r => r["Prefix"].AsString == prefix && r["Link"].AsString == link);
                        var row = rows?.FirstOrDefault();
                        if (row == null) return null;
                        return EsentSerializer.Deserialize<Article>(row)?.Html;
                    }
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                    return null;
                }
            }) as string;
        }

        public void Put(Article item)
        {
            Query(db =>
            {
                try
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        EsentCell[] cells = EsentSerializer.Serialize(item, table);
                        table.Insert(cells);
                    }
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                }
            });
        }

        public void RemoveOldEntites()
        {
            Query(db =>
            {
                try
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        table.Delete(r => r["InCollection"].AsBool && (r["CreatedOn"].AsDateTime - DateTime.Now).TotalDays > Settings.Current.CacheDays);
                    }
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                }
            });
        }
    }
}
