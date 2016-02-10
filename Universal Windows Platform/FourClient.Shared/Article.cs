using FourClient.Data;
using System;
using System.Linq;

namespace FourClient
{
    public class Article
    {
        public string Prefix { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public string Avatar { get; set; }
        public string FullLink { get; set; }
        public string CommentLink { get; set; }
        public string Html { get; set; }
        public bool InCollection { get; set; }
        public DateTime CreatedOn { get; set; }
                        
        public static Article Build(FeedItem item)
            => item == null ? null : new Article
            {
                Prefix = IoC.FeedView.CurrentSource.Prefix,
                Title = item.Title,
                Image = item.Image,
                Link = item.Link,
                Avatar = item.Avatar,
                FullLink = item.FullLink,
                CommentLink = item.CommentLink
            };

        public static Article BuildNew(FeedItem item)
        {
            var args = item.Link.Split(';').ToList();
            while (args.Count() > 2)
            {
                args[1] += ';' + args[2];
                args.Remove(args[2]);
            }
            return new Article
            {
                Prefix = args[0],
                Title = item.Title,
                Image = item.Image,
                Link = args[1],
                Avatar = item.Avatar,
                FullLink = item.FullLink,
                CommentLink = item.CommentLink
            };
        }

        public override bool Equals(object obj)
        {
            var article = obj as Article;
            if (article == null) return false;
            return article.Prefix == Prefix && article.Link == Link;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
