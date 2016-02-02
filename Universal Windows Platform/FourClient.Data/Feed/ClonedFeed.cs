namespace FourClient.Data.Feed
{
    internal class ClonedFeed : AbstractFeed
    {
        public ClonedFeed(AbstractFeed feed)
        {
            Source = feed.Source;
            SearchMode = feed.SearchMode;
            Topic = feed.Topic;
            SearchTerm = feed.SearchTerm;
        }
    }
}
