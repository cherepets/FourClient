using FourClient.Data;
using FourClient.Library.Notifications;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Notifications;
using System;
using FourClient.Library.Statistics;
using FourClient.Library.Cache;

namespace FourClient.Library
{
    public static class Notifier
    {
        private static TileUpdater TileManager => TileUpdateManager.CreateTileUpdaterForApplication();
        private static ToastNotifier ToastManager => ToastNotificationManager.CreateToastNotifier();

        public static async void UpdateMainTile(IEnumerable<FeedItem> items)
        {
            try
            {
                DisableMainTile();
                var itemArray = items.ToArray();
                if (itemArray.Length < 2) return;
                var vm = new PrimaryTileViewModel
                {
                    Title1 = itemArray[0].Title,
                    Image1 = itemArray[0].Image,
                    Avatar1 = itemArray[0].Avatar,
                    Title2 = itemArray[1].Title,
                    Image2 = itemArray[1].Image,
                    Avatar2 = itemArray[1].Avatar
                };
                var tile = await PrimaryTile.CreateInstanceAsync();
                tile.DataContext = vm;
                var notification = tile.CreateNotification();
                TileManager.Update(notification);
            }
            catch { }
        }

        public static void DisableMainTile()
        {
            var manager = TileManager;
            var notifications = manager.GetScheduledTileNotifications();
            foreach (var item in notifications)
            {
                manager.RemoveFromSchedule(item);
            }
            manager.StopPeriodicUpdate();
            manager.Clear();
            manager.EnableNotificationQueue(false);
        }

        public static async void ShowReminderToast(IEnumerable<FeedItem> items)
        {
            try
            {
                await StatisticsBase.InitAsync(true);
                await CacheBase.InitAsync(true);
                var launchStat = new LaunchStatistics();
                var keywordStat = new KeywordStatistics();
                var cache = new ArticleCache();
                var articles = items.Select(i => Article.BuildNew(i)).ToList();
                articles.RemoveAll(a => cache.FindInCache(a.Prefix, a.Link) != null);
                if (!articles.Any()) return;
                articles = articles.OrderBy(a => keywordStat.Score(a.Title)).ToList();
  //              if (launchStat.Score(DateTime.Now))
                {
                    var vm = new RemindToastViewModel
                    {
                        Title = articles[0].Title,
                        Image = articles[0].Image,
                        Avatar = articles[0].Avatar,
                        Link = articles[0].Prefix + ";" + articles[0].Link
                    };
                    var toast = new RemindToast
                    {
                        DataContext = vm
                    };
                }
            }
            catch { }
        }
    }
}
