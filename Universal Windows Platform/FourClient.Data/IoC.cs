using FourClient.Data.Interfaces;

namespace FourClient.Data
{
    public static class IoC
    {
        internal static ICache<Source> SourceCache { get; private set; }
        internal static ICache<FeedItem> TopCache { get; private set; }

        public static void RegisterDependencies(ICache<Source> sourceCache, ICache<FeedItem> topCache)
        {
            SourceCache = sourceCache;
            TopCache = topCache;
        }
    }
}
