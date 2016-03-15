using FourClient.Library.Cache;
using FourClient.Library.Statistics;
using FourClient.Views;

namespace FourClient
{
    public class IoC
    {
        public static IMainPage MainPage;
        public static IMenuView MenuView;
        public static IInterestingView InterestingView;
        public static ISourcesView SourcesView;
        public static IFeedView FeedView;
        public static IArticleView ArticleView;
        public static ICollectionView CollectionView;
        public static IArticleCache ArticleCache;
        public static IKeywordStatistics KeywordStatistics;
        public static ILaunchStatistics LaunchStatistics;

        private static bool _registred;
        private static readonly object Lock = new object();

        public static void RegisterDependencies()
        {
            lock (Lock)
            {
                if (_registred) return;
                FourClient.MainPage.PageLoaded += sender => MainPage = sender;
                Views.MenuView.ViewLoaded += sender => MenuView = sender;
                Views.InterestingView.ViewLoaded += sender => InterestingView = sender;
                Views.SourcesView.ViewLoaded += sender => SourcesView = sender;
                Views.FeedView.ViewLoaded += sender => FeedView = sender;
                Views.ArticleView.ViewLoaded += sender => ArticleView = sender;
                Views.CollectionView.ViewLoaded += sender => CollectionView = sender;
                ArticleCache = new ArticleCache();
                KeywordStatistics = new KeywordStatistics();
                LaunchStatistics = new LaunchStatistics();
                _registred = true;
            }
        }
    }
}
