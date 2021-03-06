﻿using FourClient.Data;
using FourClient.Library.Cache;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace FourClient.Library
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
                        
        public static Article Build(FeedItem item, string prefix)
            => item == null ? null : new Article
            {
                Prefix = prefix,
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

        public async void OpenWeb()
        {
            var uri = new Uri(FullLink);
            await Launcher.LaunchUriAsync(uri);
        }

        public async void OpenComments()
        {
            var uri = new Uri(CommentLink);
            await Launcher.LaunchUriAsync(uri);
        }

        public void Share()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareDataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = Title;
            request.Data.Properties.Description = "Отправлено из FourClient для Windows 10";
            try
            {
                var uri = new Uri(FullLink);
                request.Data.SetWebLink(uri);
            }
            finally
            {
                deferral.Complete();
            }
        }

        public void FillFromCache(TopCache cache)
        {
            var item = cache.FindInCache($"{Prefix};{Link}");
            Avatar = item.Avatar;
            CommentLink = item.CommentLink;
            FullLink = item.FullLink;
            Image = item.Image;
            Title = item.Title;
        }

        public async Task PreloadAsync(IArticleCache cache)
        {
            var existent = cache.FindInCollection(Prefix, Link)
                ?? cache.FindInCache(Prefix, Link);
            if (existent != null)
            {
                if (!existent.InCollection)
                {
                    existent.InCollection = true;
                    cache.UpdateCollectionState(existent);
                }
            }
            else
            {
                Html = await Api.GetArticleAsync(Prefix, Link);
                InCollection = true;
                cache.Put(this);
            }
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
