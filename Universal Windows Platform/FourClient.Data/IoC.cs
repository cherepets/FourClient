using FourClient.Data.Interfaces;

namespace FourClient.Data
{
    public static class IoC
    {
        public static ISourceSelector SourceSelector { get; private set; }
        public static ICache<Source> SourceCache { get; private set; }
        public static ICache<FeedItem> TopCache { get; private set; }

        public static void RegisterDependencies(ISourceSelector sourceSelector, ICache<Source> sourceCache, ICache<FeedItem> topCache)
        {
            SourceSelector = sourceSelector;
            SourceCache = sourceCache;
            TopCache = topCache;
        }
    }
}
