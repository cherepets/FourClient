using FourClient.Data;
using FourClient.Data.Interfaces;
using FourToolkit.Esent;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Library.Cache
{
    public class TopCache : CacheBase, ICache<FeedItem>
    {
        private const string Table = "TopCache";

        public List<FeedItem> Get()
        {
            return Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    var rows = table.Where(r => true);
                    return rows?.Select(r => EsentSerializer.Deserialize<FeedItem>(r)).ToList();
                }
            }) as List<FeedItem>;
        }

        public FeedItem FindInCache(string link)
        {
            return Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    var rows = table.Where(r => r["Link"].AsString == link);
                    var row = rows?.FirstOrDefault();
                    if (row == null) return null;
                    return EsentSerializer.Deserialize<FeedItem>(row);
                }
            }) as FeedItem;
        }

        public void Put(List<FeedItem> values)
        {
            Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    table.Delete(r => true);
                    foreach (var item in values)
                    {
                        EsentCell[] cells = EsentSerializer.Serialize(item, table);
                        table.Insert(cells);
                    }
                }
            });
        }
    }
}
