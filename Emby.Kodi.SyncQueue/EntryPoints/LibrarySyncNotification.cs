using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Kodi.SyncQueue.Data;
using Emby.Kodi.SyncQueue.Entities;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.Audio;

namespace Emby.Kodi.SyncQueue.EntryPoints
{
    public class LibrarySyncNotification : IServerEntryPoint
    {
        /// <summary>
        /// The _library manager
        /// </summary>
        private readonly ILibraryManager _libraryManager;

        private readonly ISessionManager _sessionManager;
        private readonly IUserManager _userManager;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IApplicationPaths _applicationPaths;

        /// <summary>
        /// The _library changed sync lock
        /// </summary>
        private readonly object _libraryChangedSyncLock = new object();

        private readonly List<ItemRec> _itemsRemoved = new List<ItemRec>();

        /// <summary>
        /// Gets or sets the library update timer.
        /// </summary>
        /// <value>The library update timer.</value>
        private Timer LibraryUpdateTimer { get; set; }

        /// <summary>
        /// The library update duration
        /// </summary>
        private const int LibraryUpdateDuration = 5000;

        public LibrarySyncNotification(ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, ILogger logger, IJsonSerializer jsonSerializer, IApplicationPaths applicationPaths)
        {
            _libraryManager = libraryManager;
            _sessionManager = sessionManager;
            _userManager = userManager;
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _applicationPaths = applicationPaths;

        }


        public void Run()
        {
            _libraryManager.ItemRemoved += libraryManager_ItemRemoved;
        }

        /// <summary>
        /// Handles the ItemRemoved event of the libraryManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemChangeEventArgs"/> instance containing the event data.</param>
        void libraryManager_ItemRemoved(object sender, ItemChangeEventArgs e)
        {
            if (!FilterRemovedItem(e.Item))
            {
                return;
            }

            lock (_libraryChangedSyncLock)
            {
                if (LibraryUpdateTimer == null)
                {
                    LibraryUpdateTimer = new Timer(LibraryUpdateTimerCallback, null, LibraryUpdateDuration,
                                                   Timeout.Infinite);
                }
                else
                {
                    LibraryUpdateTimer.Change(LibraryUpdateDuration, Timeout.Infinite);
                }

                var item = new ItemRec()
                {
                    ItemId = e.Item.GetClientId(),
                    LastModified = (long)(DateTimeOffset.UtcNow.Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds),
                    Status = 2
                };

                _logger.Debug(string.Format("Emby.Kodi.SyncQueue: ItemRemoved added for DB Saving {0}", e.Item.Id));
                _itemsRemoved.Add(item);
            }
        }

        /// <summary>
        /// Libraries the update timer callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private void LibraryUpdateTimerCallback(object state)
        {

            lock (_libraryChangedSyncLock)
            {

                // Remove dupes in case some were saved multiple times
                try
                {
                    DbRepo.Instance.WriteLibrarySync(_itemsRemoved.ToList());

                    if (LibraryUpdateTimer != null)
                    {
                        LibraryUpdateTimer.Dispose();
                        LibraryUpdateTimer = null;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(String.Format("Emby.Kodi.SyncQueue: An Error Has Occurred in LibraryUpdateTimerCallback: {0}", e.Message));
                    _logger.ErrorException(e.Message, e);
                }
                _itemsRemoved.Clear();
            }
        }

        private bool FilterRemovedItem(BaseItem item)
        {
            var type = item.GetType();

            return type == typeof(Movie) ||
                type == typeof(Folder) ||
                type == typeof(BoxSet) ||
                type == typeof(Series) ||
                type == typeof(Season) ||
                type == typeof(Episode) ||
                type == typeof(Audio) ||
                type == typeof(MusicArtist) ||
                type == typeof(MusicAlbum) ||
                type == typeof(MusicVideo);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (LibraryUpdateTimer != null)
            {
                LibraryUpdateTimer.Dispose();
                LibraryUpdateTimer = null;
            }

            _libraryManager.ItemRemoved -= libraryManager_ItemRemoved;
        }
    }
}
