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

        public static void RegenerateDummies()
        {
            new PrimaryTile().RegenerateDummy();
            new RemindToast().RegenerateDummy();
        }

        public static void UpdateMainTile(IEnumerable<FeedItem> items)
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
                var tile = new PrimaryTile
                {
                    DataContext = vm
                };
                var notification = tile.CreateBackgroundNotification();
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
                var articleCache = new ArticleCache();
                var topCache = new TopCache();
                topCache.Put(items.ToList());
                var articles = items.Select(i => Article.BuildNew(i)).ToList();
                articles.RemoveAll(a => articleCache.FindInCache(a.Prefix, a.Link) != null);
                articles = articles.OrderBy(a => keywordStat.Score(a.Title)).ToList();
                // Check conditions
                if (!articles.Any())
                    return;
                //if (Settings.Current.LastNotification == articles[0].Title)
                //    return;
                //if (!launchStat.Score(DateTime.Now))
                //    return;
                var link = $"{articles[0].Prefix};{articles[0].Link}";
                var vm = new RemindToastViewModel
                {
                    Title = articles[0].Title,
                    Image = articles[0].Image,
                    Avatar = articles[0].Avatar,
                    LaunchArgument = Query.Serialize("OpenArticle", articles[0]),
                    CollectionArgument = Query.Serialize("AddToCollection", articles[0])
                };
                var toast = new RemindToast
                {
                    DataContext = vm
                };
                var notification = toast.CreateBackgroundNotification();
                ToastManager.Show(notification);
                Settings.Current.LastNotification = articles[0].Title;
                StatisticsBase.Close();
                CacheBase.Close();
            }
            catch { }
        }
    }
}
