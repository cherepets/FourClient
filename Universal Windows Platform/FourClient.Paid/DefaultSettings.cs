using FourClient.Library;
using System.Collections.Generic;

namespace FourClient
{
    public static partial class DefaultSettings
    {
        private static Dictionary<string, object> DependentDictionary => new Dictionary<string, object>
            {
                {nameof(Settings.Toast), true},
                {nameof(Settings.EnableFlipViewer), true},
                {nameof(Settings.YouTube), "vnd.youtube:"}
            };
    }
}
