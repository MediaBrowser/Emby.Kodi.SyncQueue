using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Common.Configuration;
using Emby.Kodi.SyncQueue.Entities;
using Emby.Kodi.SyncQueue.Data;
using System.Globalization;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Services;

namespace Emby.Kodi.SyncQueue.API
{
    public class SyncAPI : IService
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IApplicationPaths _applicationPaths;
        private readonly IUserManager _userManager;
        private readonly ILibraryManager _libraryManager;

        //private DbRepo dbRepo = null;
        //private DataHelper dataHelper;

        public SyncAPI(ILogger logger, IJsonSerializer jsonSerializer, IApplicationPaths applicationPaths, IUserManager userManager, ILibraryManager libraryManager)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _applicationPaths = applicationPaths;
            _userManager = userManager;
            _libraryManager = libraryManager;

            _logger.Info("Emby.Kodi.SyncQueue:  SyncAPI Created and Listening at \"/Emby.Kodi.SyncQueue/{UserID}/{LastUpdateDT}/GetItems?format=json\" - {LastUpdateDT} must be a UTC DateTime formatted as yyyy-MM-ddTHH:mm:ssZ");
            _logger.Info("Emby.Kodi.SyncQueue:  SyncAPI Created and Listening at \"/Emby.Kodi.SyncQueue/{UserID}/GetItems?LastUpdateDT={LastUpdateDT}&format=json\" - {LastUpdateDT} must be a UTC DateTime formatted as yyyy-MM-ddTHH:mm:ssZ");
            _logger.Info("Emby.Kodi.SyncQueue:  The following parameters also exist to filter the results:");
            _logger.Info("Emby.Kodi.SyncQueue:  filter=movies,tvshows,music,musicvideos,boxsets");
            _logger.Info("Emby.Kodi.SyncQueue:  Results will be included by default and only filtered if added to the filter query...");
            _logger.Info("Emby.Kodi.SyncQueue:  the filter query must be lowercase in both the name and the items...");
        }

        public SyncUpdateInfo Get(GetLibraryItemsQuery request)
        {
            _logger.Info(String.Format("Emby.Kodi.SyncQueue:  Sync Requested for UserID: '{0}' with LastUpdateDT: '{1}'", request.UserID, request.LastUpdateDT));
            _logger.Debug("Emby.Kodi.SyncQueue:  Processing message...");
            if (request.LastUpdateDT == null || request.LastUpdateDT == "")
                request.LastUpdateDT = "1900-01-01T00:00:00Z";
            bool movies = true;
            bool tvshows = true;
            bool music = true;
            bool musicvideos = true;
            bool boxsets = true;

            if (request.filter != null && request.filter != "")
            {
                var filter = request.filter.ToLower().Split(',');
                foreach (var f in filter)
                {
                    f.Trim();
                    switch (f)
                    {
                        case "movies":
                            movies = false;
                            break;
                        case "tvshows":
                            tvshows = false;
                            break;
                        case "music":
                            music = false;
                            break;
                        case "musicvideos":
                            musicvideos = false;
                            break;
                        case "boxsets":
                            boxsets = false;
                            break;
                    }
                }
            }

            return PopulateLibraryInfo(
                                                            request.LastUpdateDT,
                                                            movies,
                                                            tvshows,
                                                            music,
                                                            musicvideos,
                                                            boxsets
                                                        );
        }        

        public SyncUpdateInfo PopulateLibraryInfo(string lastDT, 
                                                              bool movies, bool tvshows, bool music,
                                                              bool musicvideos, bool boxsets)
        {
            var info = new SyncUpdateInfo();

            //var userDT = Convert.ToDateTime(lastDT);
            var userDT = DateTimeOffset.Parse(lastDT, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

            var dtl = (long)(userDT.ToUniversalTime().Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

            info.ItemsRemoved = DbRepo.Instance.GetItems(dtl, 2, movies, tvshows, music, musicvideos, boxsets);

            return info;
        }
    }
}
