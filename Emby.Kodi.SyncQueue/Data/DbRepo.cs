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
    public class DbRepo: IDisposable
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

            itemRecs = NanoApi.JsonFile<ItemRec>.GetInstance(dataPath, dbItem, Encoding.UTF8, null, null);
            if (!itemRecs.CheckVersion("1.4.0"))
                itemRecs.ChangeHeader("1.4.0", "Item Repository", "This repository stores item changes per user as pushed from Emby.");         
        }

        public List<string> GetItems(long dtl, int status, bool movies, bool tvshows, bool music, bool musicvideos, bool boxsets)
        {
            var result = new List<string>();
            List<ItemRec> final = new List<ItemRec>();

            var items = itemRecs.Select(x => x.LastModified > dtl && x.Status == status).ToList();

            result = items.Where(x =>
                            {
                                switch (x.MediaType)
                                {
                                    case 0:
                                        if (movies) { return true; } else { return false; }
                                    case 1:
                                        if (tvshows) { return true; } else { return false; }
                                    case 2:
                                        if (music) { return true; } else { return false; }
                                    case 3:
                                        if (musicvideos) { return true; } else { return false; }
                                    case 4:
                                        if (boxsets) { return true; } else { return false; }
                                }
                                return false;
                            }).Select(i => i.ItemId).Distinct()
                            .ToList();

            return result;
        }

        public void DeleteOldData(long dtl)
        {
            lock (_itemLock)
            {
                itemRecs.Delete(x => x.LastModified < dtl);
            }
        }

        public void WriteLibrarySync(List<LibItem> Items, int status)
        {
            ItemRec newRec;
            var statusType = string.Empty;
            if (status == 0) { statusType = "Added"; }
            else if (status == 1) { statusType = "Updated"; }
            else { statusType = "Removed"; }

            lock (_itemLock)
            {
                var newRecs = new List<ItemRec>();
                var upRecs = new List<ItemRec>();

                foreach (var i in Items)
                {
                    var newTime = i.SyncApiModified;

                    var rec = itemRecs.Select(x => x.ItemId == i.Id).FirstOrDefault();

                    newRec = new ItemRec()
                    {
                        ItemId = i.Id,
                        Status = status,
                        LastModified = newTime,
                        MediaType = i.ItemType
                    };

                    if (rec == null) { newRecs.Add(newRec); } 
                    else if (rec.LastModified < newTime)
                    {
                        newRec.Id = rec.Id;
                        upRecs.Add(newRec);
                    }

                    if (newRec != null)
                    {
                        logger.Debug(String.Format("Emby.Kodi.SyncQueue:  {0} ItemId: '{1}'", statusType, newRec.ItemId));
                    }
                    else
                    {
                        logger.Debug(String.Format("Emby.Kodi.SyncQueue:  ItemId: '{0}' Skipped", i.Id));
                    }
                }

                if (newRecs.Count > 0)
                {

                    itemRecs.Insert(newRecs);

                }
                if (upRecs.Count > 0)
                {
                    var data = itemRecs.Select();


                    foreach (var rec in upRecs)
                    {
                        data.Where(d => d.Id == rec.Id).ToList().ForEach(i =>
                        {
                            i.ItemId = rec.ItemId;
                            i.Status = rec.Status;
                            i.LastModified = rec.LastModified;
                            i.MediaType = rec.MediaType;
                        });
                    }

                    itemRecs.Commit(data);

                    data = itemRecs.Select();
                }
            }
        }

        #region Dispose

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

        #endregion
    }
}
