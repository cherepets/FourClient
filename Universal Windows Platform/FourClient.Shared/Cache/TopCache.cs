using System;
using FourToolkit.Esent;
using FourClient.Data.Interfaces;
using FourClient.Data;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Cache
{
    public class TopCache : Esent, ICache<FeedItem>
    {
        private const string Table = "TopCache";

        public List<FeedItem> Get()
        {
            return Query(db =>
            {
                try
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        var rows = table.Where(r => true);
                        return rows?.Select(r => EsentSerializer.Deserialize<FeedItem>(r)).ToList();
                    }
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                    return null;
                }
            }) as List<FeedItem>;
        }
        
        public void Put(List<FeedItem> values)
        {
            Query(db =>
            {
                try
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
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                }
            });
        }
    }
}
