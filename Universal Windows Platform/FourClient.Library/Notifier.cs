using FourClient.Data;
using FourClient.Library.Notifications;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Notifications;

namespace FourClient.Library
{
    public static class Notifier
    {
        private static TileUpdater MainTileUpdater => TileUpdateManager.CreateTileUpdaterForApplication();
        
        public static void UpdateMainTile(IEnumerable<FeedItem> items)
        {
            try
            {
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
                var notification = tile.CreateNotification();
                MainTileUpdater.Update(notification);
            }
            catch { }
        }

        public static void DisableMainTile()
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            var notifications = updater.GetScheduledTileNotifications();
            foreach (var item in notifications)
            {
                updater.RemoveFromSchedule(item);
            }
            updater.StopPeriodicUpdate();
            updater.Clear();
            updater.EnableNotificationQueue(false);
        }
    }
}
