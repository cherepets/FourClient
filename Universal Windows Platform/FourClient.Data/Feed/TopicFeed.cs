namespace FourClient.Data.Feed
{
    public class TopicFeed : AbstractFeed
    {
        public new Source Source
        {
            get { return base.Source; }
            set
            {
                base.Source = value;
                SearchMode = false;
            }
        }
    }

    public class TopicFeedAccessor
    {
        internal Source Source { get; }

        internal TopicFeedAccessor(Source source) { Source = source; }
        public AbstractFeed this[string topic] => new TopicFeed
        {
            Source = Source,
            Topic = topic
        };
    }
}
