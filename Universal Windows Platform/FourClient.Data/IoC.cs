using FourClient.Data.Interfaces;

namespace FourClient.Data
{
    public static class IoC
    {
        internal static ISourceSelector SourceSelector { get; private set; }
        internal static ICache<Source> SourceCache { get; private set; }
        internal static ICache<FeedItem> TopCache { get; private set; }

        public static void RegisterDependencies(ISourceSelector sourceSelector, ICache<Source> sourceCache, ICache<FeedItem> topCache)
        {
            SourceSelector = sourceSelector;
            SourceCache = sourceCache;
            TopCache = topCache;
        }
    }
}
