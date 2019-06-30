using MediaBrowser.Controller.Entities;
using System;

namespace Emby.Kodi.SyncQueue.Entities
{
    public class LibItem
    {
        public string Id { get; set; }
        public long SyncApiModified { get; set; }

    }
}
