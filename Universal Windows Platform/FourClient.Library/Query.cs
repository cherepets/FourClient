using Microsoft.QueryStringDotNET;
using System;

namespace FourClient.Library
{
    public static class Query
    {
        public static string Serialize(string action, Article article)
        {
            return Encode(new QueryString
            {
                { "action", action },
                { nameof (Article.Prefix), article.Prefix },
                { nameof (Article.Link), article.Link }
            }
            .ToString());
        }

        public static Tuple<string, Article> Deserialize(string s)
        {
            var q = QueryString.Parse(Decode(s));
            return new Tuple<string, Article>
                (q["action"],
                new Article
                {
                    Prefix = q[nameof(Article.Prefix)],
                    Link = q[nameof(Article.Link)]
                });
        }

        private static string Encode(string s)
            => s
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("\'", "&apos;")
            .Replace("&", "&amp;");

        private static string Decode(string s)
            => s
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "\'")
            .Replace("&amp;", "&");
    }
}
