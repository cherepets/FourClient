using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using FourToolkit.Esent;

namespace FourClient.Cache
{
    public class Esent
    {
        public static EsentInstance Instance;
        public static EsentDatabase Database;

        private const string DbFolderName = "esent";
        private const string DbFileName = "FourClientCache.db";
        protected static string DbPath = Path.Combine(LocalFolder.Path, DbFolderName, DbFileName);

        private static StorageFolder AppxFolder => Package.Current.InstalledLocation;
        private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

        private static bool _inited;
        private static readonly object Lock = new object();

        public Esent()
        {
            Init();
        }

        public static async Task InitAsync()
        {
            lock (Lock)
            {
                if (_inited) return;
                _inited = true;
            }
            if (!await CopyFileIfNeeded())
            {
                _inited = false;
                return;
            }
            try
            {
                Instance = new EsentInstance("EsentManagementStudio");
                using (var session = Instance.BeginSession())
                using (var db = session.OpenDatabase(DbPath))
                    db.Close();
            }
            catch
            {
                Instance.Close();
                await CopyFileAsync();
                Instance = new EsentInstance("EsentManagementStudio");
            }
        }

        private static async void Init() => await InitAsync();

        public static object Query(Func<EsentDatabase, object> func, bool throwException = false)
        {
            lock (Lock)
            {
                try
                {
                    using (var session = Instance.BeginSession())
                    using (var db = session.OpenDatabase(DbPath))
                        return func.Invoke(db);
                }
                catch
                {
                    if (throwException) throw;
                    return null;
                }
            }
        }

        public static void Query(Action<EsentDatabase> action, bool throwException = false)
        {
            lock (Lock)
            {
                try
                {
                    using (var session = Instance.BeginSession())
                    using (var db = session.OpenDatabase(DbPath))
                        action.Invoke(db);
                }
                catch
                {
                    if (throwException) throw;
                }
            }
        }

        public static void Close()
        {
            _inited = false;
            Instance?.Close();
        }

        private static async Task<bool> CopyFileIfNeeded()
        {
            var file = await GetDbFileAsync();
            if (file != null) return true;
            return await CopyFileAsync();
        }

        private static async Task<bool> CopyFileAsync()
        {
            var folders = await AppxFolder.GetFoldersAsync();
            var folder = folders.FirstOrDefault(q => q.Name == DbFolderName);
            if (folder == null) return false;
            var files = await folder.GetFilesAsync();
            var file = files.FirstOrDefault(q => q.Name == DbFileName);
            var targetFolder = await GetDbFolderAsync() ?? await LocalFolder.CreateFolderAsync(DbFolderName);
            await file.CopyAsync(targetFolder, DbFileName, NameCollisionOption.ReplaceExisting);
            return true;
        }

        private static async Task<StorageFile> GetDbFileAsync()
        {
            var dbFolder = await GetDbFolderAsync();
            if (dbFolder == null) return null;
            var files = await dbFolder.GetFilesAsync();
            var file = files.FirstOrDefault(q => q.Name == DbFileName);
            return file;
        }

        private static async Task<StorageFolder> GetDbFolderAsync()
        {
            var folders = await LocalFolder.GetFoldersAsync();
            var folder = folders.FirstOrDefault(q => q.Name == DbFolderName);
            return folder;
        }
    }
}
