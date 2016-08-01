using FourClient.Library;
using System.Collections.Generic;

namespace FourClient
{
    public static partial class DefaultSettings
    {
        private static Dictionary<string, object> DependentDictionary => new Dictionary<string, object>
            {
                {nameof(Settings.Toast), false},
                {nameof(Settings.EnableFlipViewer), false},
                {nameof(Settings.YouTube), "http://www.youtube.com/watch?v="}
            };
    }
}
