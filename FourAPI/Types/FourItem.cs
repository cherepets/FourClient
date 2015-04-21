using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace FourAPI.Types
{
    /// <summary>
    /// Item of the list of articles
    /// </summary>
    public struct FourItem
    {
        /// <summary>
        /// Article title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Article thumbnail url
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// Article address that used within FourAPI
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Source thumbnail url
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// Article web address
        /// </summary>
        public string FullLink { get; set; }
        /// <summary>
        /// Comments web address
        /// </summary>
        public string CommentLink { get; set; }
        /// <summary>
        /// Gets top articles
        /// </summary>
        /// <param name="cachePath">Where to save cache</param>
        /// <returns>Collection of articles</returns>
        public static async Task<ObservableCollection<FourItem>> GetTopAsync(string cachePath)
        {
            var client = new WebXslt.Client(Settings.Url);
            var collection = await client.CallAsync<FourItem>("MVW_GetPage");
            // Save cache
            if (cachePath != null)
            {
                var xroot = new XElement("Collection");
                var xdoc = new XDocument(xroot);
                foreach (var page in collection)
                {
                    var xsource =
                        new XElement("Top",
                            new XElement("CommentLink", page.CommentLink),
                            new XElement("FullLink", page.FullLink),
                            new XElement("Image", page.Image),
                            new XElement("Link", page.Link),
                            new XElement("Avatar", page.Avatar),
                            new XElement("Title", page.Title));
                    xroot.Add(xsource);
                };
                var appData = ApplicationData.Current.LocalFolder;
                var fileCreate = appData.CreateFileAsync(cachePath, CreationCollisionOption.ReplaceExisting);
                var file = await fileCreate;
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    xdoc.Save(stream);
                }
            }
            return collection;
        }
    }
}
