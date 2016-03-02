using FourClient.Library;
using FourClient.Library.Cache;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace FourClient.Background
{
    public sealed class ToastHandlerBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                var toastArgs = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                if (toastArgs == null) return;
                await CacheBase.InitAsync(true);
                var topCache = new TopCache();
                var articleCache = new ArticleCache();
                var arguments = Query.Deserialize(toastArgs.Argument);
                var article = arguments.Item2;
                article.FillFromCache(topCache);
                await article.PreloadAsync(articleCache);
                CacheBase.Close();
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}