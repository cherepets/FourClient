using System;
using FourToolkit.Esent;
using FourClient.Data.Interfaces;
using FourClient.Data;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Cache
{
    public class SourceCache : Esent, ICache<Source>
    {
        private const string Table = "SourceCache";

        public List<Source> Get()
        {
            return Query(db =>
            {
                try
                {
                    using (var table = db.Tables[Table].Open())
                    {
                        var rows = table.Where(r => true);
                        return rows?.Select(r => EsentSerializer.Deserialize<Source>(r)).ToList();
                    }
                }
                catch (Exception exception)
                {
                    App.HandleException(exception);
                    return null;
                }
            }) as List<Source>;
        }

        public void Put(List<Source> values)
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
