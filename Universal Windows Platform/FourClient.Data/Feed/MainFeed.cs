using System.Linq;

namespace FourClient.Data.Feed
{
    public class MainFeed : AbstractFeed
    {
        public new Source Source
        {
            get { return base.Source; }
            set
            {
                base.Source = value;
                Topic = Source.Topics.First().Key;
                SearchMode = false;
            }
        }
    }
}
