using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using Emby.Kodi.SyncQueue.Entities;
using NanoApi.Entities;
using System.Text;
using MediaBrowser.Model.IO;
using System.Globalization;

namespace Emby.Kodi.SyncQueue.Data
{
    public class DbRepo : IDisposable
    {
        private readonly object _itemLock = new object();

        private const string dbItem = "Emby.Kodi.SyncQueue.I.1.40.json";

        private string dataPath = "";

        public string DataPath
        {
            get { return dataPath; }
            set { dataPath = Path.Combine(value, "SyncData"); }
        }

        private NanoApi.JsonFile<ItemRec> itemRecs = null;

        private static DbRepo instance = null;
        public static ILogger logger = null;
        public static IJsonSerializer json = null;
        public static string dbPath = "";
        public static IFileSystem fileSystem = null;

        public static DbRepo Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DbRepo(dbPath);
                }
                return instance;
            }
        }


        public DbRepo(string dPath)
        {
            logger.Info("Emby.Kodi.SyncQueue: Creating DB Repository...");
            this.DataPath = dPath;

            fileSystem.CreateDirectory(dataPath);

            itemRecs = new NanoApi.JsonFile<ItemRec>(dataPath, dbItem, Encoding.UTF8, logger);
        }

        public List<string> GetItems(long dtl)
        {
            return itemRecs
                .Select(x => x.LastModified > dtl)
                .Select(i => i.ItemId)
                .ToList();
        }

        public void DeleteOldData(long dtl)
        {
            lock (_itemLock)
            {
                itemRecs.Delete(x => x.LastModified < dtl);
            }
        }

        public void WriteLibrarySync(List<ItemRec> Items)
        {
            if (Items.Count > 0)
            {
                lock (_itemLock)
                {
                    itemRecs.Insert(Items);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                if (itemRecs != null)
                {
                    itemRecs.Dispose();
                    itemRecs = null;
                }
            }
        }
    }
}
