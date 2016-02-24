using FourClient.Data;
using FourClient.Data.Interfaces;
using FourToolkit.Esent;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Library.Cache
{
    public class SourceCache : CacheBase, ICache<Source>
    {
        private const string Table = "SourceCache";

        public List<Source> Get()
        {
            return Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    var rows = table.Where(r => true);
                    return rows?.Select(r => EsentSerializer.Deserialize<Source>(r)).ToList();
                }
            }) as List<Source>;
        }

        public void Put(List<Source> values)
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
