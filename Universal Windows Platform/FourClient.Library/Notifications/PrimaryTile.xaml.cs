using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FourClient.Library.Notifications
{
    public sealed partial class PrimaryTile
    {
        public void RegenerateDummy()
        {
            InitializeComponent();
            var vm = ViewModelHelper.GenerateDummy(new PrimaryTileViewModel());
            DataContext = vm;
            Settings.Current.DummyPrimaryTile = CreateNotification().Content.GetXml();
        }

        public TileNotification CreateBackgroundNotification()
        {
            var vm = DataContext as PrimaryTileViewModel;
            var xml = ViewModelHelper.FillXml(vm, Settings.Current.DummyPrimaryTile);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return new TileNotification(doc);
        }
    }
}
