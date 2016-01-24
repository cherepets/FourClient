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
                        return rows?.Select(r => new Source
                        {
                            Prefix = r["Prefix"].AsString,
                            Enabled = r["Enabled"].AsBool,
                            Base64Types = r["Base64Types"].AsString,
                            ImageUrl = r["ImageUrl"].AsString,
                            Name = r["Name"].AsString,
                            Searchable = r["Searchable"].AsBool,
                        }).ToList();
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
                            var cells = new[]
                            {
                                new EsentCell("Prefix", item.Prefix),
                                new EsentCell("Enabled", item.Enabled),
                                new EsentCell("Base64Types", item.Base64Types),
                                new EsentCell("ImageUrl", item.ImageUrl),
                                new EsentCell("Name", item.Name),
                                new EsentCell("Searchable", item.Searchable),
                            };
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
