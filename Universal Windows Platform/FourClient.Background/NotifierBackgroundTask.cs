using FourClient.Data;
using FourClient.Library;
using Windows.ApplicationModel.Background;

namespace FourClient.Background
{
    public sealed class NotifierBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (Settings.Current.LiveTile)
            {
                var top = Api.GetTop();
                Notifier.UpdateMainTile(top);
            }
        }
    }
}
