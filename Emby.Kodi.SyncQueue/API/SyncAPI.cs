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
        [ApiMember(Name = "LastUpdateDT", Description = "UTC DateTime of Last Update, Format yyyy-MM-ddTHH:mm:ssZ", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        [ApiMember(Name = "filter", Description = "Comma separated list of Collection Types to filter (movies,tvshows,music,musicvideos,boxsets", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserID { get; set; }
        public string LastUpdateDT { get; set; }

    }

    public class SyncAPI : IService
    {
        public SyncUpdateInfo Get(GetLibraryItemsQuery request)
        {
            if (request.LastUpdateDT == null || request.LastUpdateDT == "")
                request.LastUpdateDT = "1900-01-01T00:00:00Z";

            return PopulateLibraryInfo(request.LastUpdateDT);
        }        

        public SyncUpdateInfo PopulateLibraryInfo(string lastDT)
        {
            var info = new SyncUpdateInfo();

            //var userDT = Convert.ToDateTime(lastDT);
            var userDT = DateTimeOffset.Parse(lastDT, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

            var dtl = (long)(userDT.ToUniversalTime().Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

            info.ItemsRemoved = DbRepo.Instance.GetItems(dtl);

            return info;
        }
    }
}
