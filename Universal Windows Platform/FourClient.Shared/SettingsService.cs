using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace FourClient
{
    public static class SettingsService
    {
        public static bool LargeScreen => Window.Current.Bounds.Width >= 720 || Window.Current.Bounds.Height >= 720;
        public static bool IsPhone => ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
        public static string ShareTemplate => "http://fourclient.azurewebsites.net/index.aspx?page=article&prefix={0}&link={1}&title={2}";

        public static string DisplayVersion => Package.Current.Id.Version.ToString();

        public static ElementTheme GetMainTheme() => MainTheme ? ElementTheme.Light : ElementTheme.Dark;
        public static ElementTheme GetArticleTheme() => ArticleTheme ? ElementTheme.Light : ElementTheme.Dark;
        public static Color GetStatusForeground() => MainTheme ? Colors.Black : Colors.White;

        public static bool MainTheme { get; private set; }
        public static bool ArticleTheme { get; private set; }
        public static bool FirstRun { get; private set; }
        public static bool LiveTile { get; private set; }
        public static bool RenderSwitch { get; private set; }
        public static bool UpperMenu { get; private set; }
        public static int FontSize { get; private set; }
        public static string FontFace { get; private set; }
        public static string Align { get; private set; }
        public static string YouTube { get; private set; }
        public static string Render { get; private set; }

        static SettingsService()
        {
            LoadSettings();
        }
        public static void SetMainTheme(bool theme)
        {
            MainTheme = theme;
            ApplicationData.Current.LocalSettings.Values["MainTheme"] = theme;
        }

        public static void SetArticleTheme(bool theme)
        {
            ArticleTheme = theme;
            ApplicationData.Current.LocalSettings.Values["ArticleTheme"] = theme;
        }

        public static void SetLiveTile(bool liveTile)
        {
            LiveTile = liveTile;
            ApplicationData.Current.LocalSettings.Values["LiveTile"] = liveTile;
            if (!liveTile)
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                var notifications = updater.GetScheduledTileNotifications();
                foreach (var item in notifications)
                {
                    updater.RemoveFromSchedule(item);
                }
                updater.StopPeriodicUpdate();
                updater.Clear();
                updater.EnableNotificationQueue(false);
            }
        }

        public static void SetRenderSwitch(bool renderSwitch)
        {
            RenderSwitch = renderSwitch;
            ApplicationData.Current.LocalSettings.Values["RenderSwitch"] = renderSwitch;
        }

        public static void SetUpperMenu(bool upperMenu)
        {
            UpperMenu = upperMenu;
            ApplicationData.Current.LocalSettings.Values["UpperMenu"] = upperMenu;
            MainPage.RebuildNewsFeedUI();
        }

        public static void SetFontSize(int fontSize)
        {
            FontSize = fontSize;
            ApplicationData.Current.LocalSettings.Values["FontSize"] = fontSize;
        }

        public static void SetFontFace(string fontFace)
        {
            FontFace = fontFace;
            ApplicationData.Current.LocalSettings.Values["FontFace"] = fontFace;
        }

        public static void SetAlign(string align)
        {
            Align = align;
            ApplicationData.Current.LocalSettings.Values["Align"] = align;
        }

        public static void SetYouTube(string youtube)
        {
            YouTube = youtube;
            ApplicationData.Current.LocalSettings.Values["YouTube"] = youtube;
        }

        public static void SetRender(string render)
        {
            Render = render;
            ApplicationData.Current.LocalSettings.Values["Render"] = render;
        }

        private static void LoadSettings()
        {
            //First Run            
            var firstRun = ApplicationData.Current.LocalSettings.Values["FirstRun"];
            FirstRun = firstRun != null ? (bool)firstRun : true;
            ApplicationData.Current.LocalSettings.Values["FirstRun"] = false;
            //Main Theme
            var mainTheme = ApplicationData.Current.LocalSettings.Values["MainTheme"];
            MainTheme = mainTheme != null ? (bool)mainTheme : false;
            //Article Theme
            var articleTheme = ApplicationData.Current.LocalSettings.Values["ArticleTheme"];
            ArticleTheme = articleTheme != null ? (bool)articleTheme : false;
            //LiveTile
            var liveTile = ApplicationData.Current.LocalSettings.Values["LiveTile"];
            LiveTile = liveTile != null ? (bool)liveTile : true;
            //RenderSwitch
            var renderSwitch = ApplicationData.Current.LocalSettings.Values["RenderSwitch"];
            RenderSwitch = renderSwitch != null ? (bool)renderSwitch : false;
            //UpperMenu
            var upperMenu = ApplicationData.Current.LocalSettings.Values["UpperMenu"];
            UpperMenu = upperMenu != null ? (bool)upperMenu : IsPhone;
            //Font Size
            var fontSize = ApplicationData.Current.LocalSettings.Values["FontSize"];
            FontSize = fontSize != null ? (int)fontSize : (IsPhone && LargeScreen ? 2 : 3);
            //Font Face
            var fontFace = ApplicationData.Current.LocalSettings.Values["FontFace"];
            FontFace = fontFace != null ? (string)fontFace : "Segoe UI";
            //Align
            var align = ApplicationData.Current.LocalSettings.Values["Align"];
            Align = align != null ? (string)align : "left";
            //YouTube
            var youtube = ApplicationData.Current.LocalSettings.Values["YouTube"];
            YouTube = youtube != null ? (string)youtube : (IsPhone ? "vnd.youtube:" : "http://www.youtube.com/watch?v=");
            //Render
            var render = ApplicationData.Current.LocalSettings.Values["Render"];
            Render = render != null ? (string)render : "EdgeHtml";
        }
    }
}
