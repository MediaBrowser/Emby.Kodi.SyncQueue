using System;
using Emby.Kodi.SyncQueue.Entities;
using Emby.Kodi.SyncQueue.Data;
using System.Globalization;
using MediaBrowser.Model.Services;

namespace Emby.Kodi.SyncQueue.API
{
    [Route("/Emby.Kodi.SyncQueue/{UserID}/GetItems", "GET", Summary = "Gets Items for {UserID} from {UTC DATETIME} formatted as yyyy-MM-ddTHH:mm:ssZ using queryString LastUpdateDT")]
    public class GetLibraryItemsQuery : IReturn<SyncUpdateInfo>
    {
        [ApiMember(Name = "UserID", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string UserID { get; set; }

        [ApiMember(Name = "LastUpdateDT", Description = "UTC DateTime of Last Update, Format yyyy-MM-ddTHH:mm:ssZ", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string LastUpdateDT { get; set; }
    }

    public class SyncAPI : IService
    {
        public SyncUpdateInfo Get(GetLibraryItemsQuery request)
        {
            if (request.LastUpdateDT == null || request.LastUpdateDT == "")
            {
                return PopulateLibraryInfo(DateTimeOffset.UtcNow.AddDays(-7));
            }

            return PopulateLibraryInfo(DateTimeOffset.Parse(request.LastUpdateDT, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal));
        }        

        public SyncUpdateInfo PopulateLibraryInfo(DateTimeOffset userDT)
        {
            var info = new SyncUpdateInfo();

            var dtl = (long)(userDT.ToUniversalTime().Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

            info.ItemsRemoved = DbRepo.Instance.GetItems(dtl);

            return info;
        }
    }
}
