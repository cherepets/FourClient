using FourToolkit.Notifications.AdaptiveTile;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Markup;

namespace FourClient.Library.Notifications
{
    public sealed partial class PrimaryTile : Tile
    {
        public PrimaryTile()
        {
        }

        public static async Task<PrimaryTile> CreateInstanceAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///FourClient.Library/Notifications/PrimaryTile.xaml"));
            var xaml = await FileIO.ReadTextAsync(file);
            var content = (PrimaryTile)XamlReader.Load(xaml);
            return content;
        }
    }
}
