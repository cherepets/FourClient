using FourClient.Data;
using FourClient.Library;
using Windows.ApplicationModel.Background;

namespace FourClient.Background
{
    public sealed class NotifierBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            try
            {
                if (!Settings.Current.LiveTile && !Settings.Current.Toast)
                    return;
                var top = Api.GetTop(false);
                if (Settings.Current.LiveTile)
                    Notifier.UpdateMainTile(top);
                if (Settings.Current.Toast)
                    Notifier.ShowReminderToast(top);
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
