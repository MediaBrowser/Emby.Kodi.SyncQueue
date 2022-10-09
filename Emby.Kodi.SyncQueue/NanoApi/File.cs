using NanoApi.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Emby.Kodi.SyncQueue.Data;
using System.Threading;
using MediaBrowser.Model.Logging;

namespace NanoApi
{
    internal class File
    {
        private string path { get; set; }
        private string filename { get; set; }
        private Encoding encoding { get; set; }
        private ILogger logger { get; set; }

        public File(string path, string filename, Encoding encoding, ILogger logger)
        {
            this.path = path;
            this.filename = filename;
            this.encoding = encoding;
            this.logger = logger;
        }

        public bool Save<T>(List<T> data)
        {
            Foo<T> foo = this.Read<T>();
            if (foo == null)
                foo = new Foo<T>();

            foo.data = data;
            return this.Save<T>(foo);
        }

        public bool Save<T>(Foo<T> foo)
        {
            string contents = DbRepo.json.SerializeToString(foo);
            DbRepo.fileSystem.CreateDirectory(this.path);
            string path = Path.Combine(this.path, this.filename);
            if (this.encoding == null)
                DbRepo.fileSystem.WriteAllText(path, contents);
            else
                DbRepo.fileSystem.WriteAllText(path, contents, this.encoding);

            return true;
        }

        public Foo<T> Read<T>()
        {
            string path = Path.Combine(this.path, this.filename);
            if (!DbRepo.fileSystem.FileExists(path))
                return new Foo<T>();

            Foo<T> foo = null;

            try
            {
                foo = DbRepo.json.DeserializeFromString<Foo<T>>(DbRepo.fileSystem.ReadAllText(path));
            }
            catch (Exception ex)
            {
                logger.Error("Error deserializing data file {0} {1}", path, ex.Message);
            }

            if (foo == null)
            {
                return new Foo<T>();
            }

            if (foo.data == null)
                foo.data = new List<T>();
            return foo;
        }
    }
}
