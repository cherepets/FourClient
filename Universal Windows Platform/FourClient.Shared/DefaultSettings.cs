using FourClient.Library;
using FourToolkit.Extensions;
using FourToolkit.UI;
using System.Collections.Generic;
using Windows.ApplicationModel;

namespace FourClient
{
    public static partial class DefaultSettings
    {
        public static readonly IDictionary<string, object> Dictionary
            = IndependentDictionary.Merge(DependentDictionary);

        private static Dictionary<string, object> IndependentDictionary => new Dictionary<string, object>
            {
                {nameof(Settings.LastVersion), new PackageVersion()},
                {nameof(Settings.AppTheme), false},
                {nameof(Settings.ArticleTheme), false},
                {nameof(Settings.LiveTile), true},
                {nameof(Settings.FilterInteresting), false},
                {nameof(Settings.AllowRotation), false},
                {nameof(Settings.ScrollEventThreshold), 16},
                {nameof(Settings.FontSize), Platform.IsMobile ? 2 : 3},
                {nameof(Settings.FontFace), "Segoe UI"},
                {nameof(Settings.Align), "left"},
                {nameof(Settings.ShowAtStartup), 0 },
                {nameof(Settings.HiddenSources), string.Empty},
                {nameof(Settings.CacheDays), 3},
            };
    }
}
