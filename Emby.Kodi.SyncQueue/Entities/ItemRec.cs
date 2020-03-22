using System;

namespace Emby.Kodi.SyncQueue.Entities
{
    public class ItemRec
    {
        //Status 0 = Added
        //Status 1 = Updated
        //Status 2 = Removed

        public string ItemId { get; set; }
        public long LastModified { get; set; }
    }
}
