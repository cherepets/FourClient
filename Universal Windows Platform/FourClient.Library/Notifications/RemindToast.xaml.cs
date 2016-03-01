using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FourClient.Library.Notifications
{
    public sealed partial class RemindToast
    {
        public void RegenerateDummy()
        {
            InitializeComponent();
            var vm = ViewModelHelper.GenerateDummy(new RemindToastViewModel());
            DataContext = vm;
            Settings.Current.DummyRemindToast = CreateNotification().Content.GetXml();
        }

        public ToastNotification CreateBackgroundNotification()
        {
            var vm = DataContext as RemindToastViewModel;
            var xml = ViewModelHelper.FillXml(vm, Settings.Current.DummyRemindToast);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return new ToastNotification(doc);
        }
    }
}
