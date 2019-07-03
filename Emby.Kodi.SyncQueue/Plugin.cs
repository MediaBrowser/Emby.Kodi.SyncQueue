using System;
using System.Collections.Generic;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Plugins;
using Emby.Kodi.SyncQueue.Data;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Drawing;
using System.IO;

namespace Emby.Kodi.SyncQueue
{
    class Plugin : BasePlugin, IHasThumbImage
    {
        public Plugin(IApplicationPaths applicationPaths, ILogger logger, IJsonSerializer json, IFileSystem fileSystem)
        {
            DbRepo.dbPath = applicationPaths.DataPath;
            DbRepo.json = json;
            DbRepo.logger = logger;
            DbRepo.fileSystem = fileSystem;
        }

        private Guid _id = new Guid("b0daa30f-2e09-4083-a6ce-459d9fecdd80");
        public override Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "Kodi companion"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return "Companion for Kodi add-ons. Provides dynamic strms and shorter sync times for Emby for Kodi.";
            }
        }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.jpg");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Jpg;
            }
        }
    }
}
