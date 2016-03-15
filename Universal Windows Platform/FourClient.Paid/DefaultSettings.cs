using FourClient.Library;
using FourToolkit.UI;
using System.Collections.Generic;
using Windows.ApplicationModel;

namespace FourClient
{
    public static class DefaultSettings
    {
        public static readonly Dictionary<string, object>  Dictionary = new Dictionary<string, object>
            {
                {nameof(Settings.LastVersion), new PackageVersion()},
                {nameof(Settings.AppTheme), false},
                {nameof(Settings.ArticleTheme), false},
                {nameof(Settings.LiveTile), true},
                {nameof(Settings.Toast), true},
                {nameof(Settings.FilterInteresting), false},
                {nameof(Settings.AllowRotation), false},
                {nameof(Settings.EnableFlipViewer), true},
                {nameof(Settings.ScrollEventThreshold), 20},
                {nameof(Settings.FontSize), Platform.IsMobile ? 2 : 3},
                {nameof(Settings.FontFace), "Segoe UI"},
                {nameof(Settings.Align), "left"},
                {nameof(Settings.ShowAtStartup), 0 },
                {nameof(Settings.YouTube), "vnd.youtube:"},
                {nameof(Settings.HiddenSources), string.Empty},
                {nameof(Settings.CacheDays), 3},
            };
    }
}
