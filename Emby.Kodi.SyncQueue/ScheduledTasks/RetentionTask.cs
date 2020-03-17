using System.Globalization;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.Kodi.SyncQueue.Data;
using MediaBrowser.Model.Tasks;

namespace Emby.Kodi.SyncQueue.ScheduledTasks
{
    public class FireRetentionTask : IScheduledTask, IConfigurableScheduledTask
    {
        public string Key
        {
            get { return "KodiSyncFireRetentionTask"; }
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[] {

                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerInterval,
                    IntervalTicks = TimeSpan.FromHours(24).Ticks
                }
            };
        }

        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            //Is retDays 0.. If So Exit...
            int retDays = 30;

            retDays = retDays * -1;
            var dt = DateTimeOffset.UtcNow.AddDays(retDays);
            var dtl = (long)(dt.Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
            //DbRepo.DeleteOldData(dtl, _logger);

            DbRepo.Instance.DeleteOldData(dtl);

            return Task.CompletedTask;
        }

        public string Name
        {
            get { return "Remove Old Sync Data"; }
        }

        public string Category
        {
            get
            {
                return "Emby.Kodi.SyncQueue";
            }
        }

        public string Description
        {
            get
            {
                return
                    "If Retention Days > 0 then this will remove the old data to keep information flowing quickly";
            }
        }

        public bool IsHidden => true;

        public bool IsEnabled => true;

        public bool IsLogged => true;
    }

}
