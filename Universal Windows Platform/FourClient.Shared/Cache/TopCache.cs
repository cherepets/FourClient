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
                        return rows?.Select(r => new FeedItem
                        {
                            Avatar = r["Avatar"].AsString,
                            CommentLink = r["CommentLink"].AsString,
                            FullLink = r["FullLink"].AsString,
                            Image = r["Image"].AsString,
                            Link = r["Link"].AsString,
                            Title = r["Title"].AsString,
                        }).ToList();
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
                            var cells = new[]
                            {
                                new EsentCell("Avatar", item.Avatar),
                                new EsentCell("CommentLink", item.CommentLink),
                                new EsentCell("FullLink", item.FullLink),
                                new EsentCell("Image", item.Image),
                                new EsentCell("Link", item.Link),
                                new EsentCell("Title", item.Title),
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
