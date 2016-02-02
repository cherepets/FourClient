namespace FourClient.Data.Feed
{
    public class SearchFeed : AbstractFeed
    {
        public new Source Source
        {
            get { return base.Source; }
            set
            {
                base.Source = value;
                SearchMode = true;
            }
        }
    }

    public class SearchFeedAccessor
    {
        internal Source Source { get; }

        internal SearchFeedAccessor(Source source) { Source = source; }
        public AbstractFeed this[string searchTerm] => new SearchFeed
        {
            Source = Source,
            SearchTerm = searchTerm
        };
    }
}
