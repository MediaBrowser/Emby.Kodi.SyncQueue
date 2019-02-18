﻿using System;
using System.Collections.Generic;
using Jellyfin.Plugin.KodiSyncQueue.Configuration;
using Jellyfin.Plugin.KodiSyncQueue.Data;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.KodiSyncQueue
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public DbRepo DbRepo { get; }

        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILogger logger,
            IJsonSerializer json
            ) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;

            logger.LogInformation("Jellyfin.Plugin.KodiSyncQueue IS NOW STARTING!!!");
            
            DbRepo = new DbRepo(applicationPaths.DataPath, logger, json);
        }

        private readonly Guid _id = new Guid("771e19d6-5385-4caf-b35c-28a0e865cf63");
        public override Guid Id => _id;

        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        /// <value>The name.</value>
        public override string Name => "Kodi companion";

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
            => "Companion for Kodi add-ons. Provides dynamic strms and shorter sync times for Jellyfin for Kodi.";

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "Jellyfin.Plugin.KodiSyncQueue",
                    EmbeddedResourcePath = "Jellyfin.Plugin.KodiSyncQueue.Configuration.configPage.html"
                }
            };
        }
    }
}
