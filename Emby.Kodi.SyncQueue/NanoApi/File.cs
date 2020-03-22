using NanoApi.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Emby.Kodi.SyncQueue.Data;
using System.Threading;

namespace NanoApi
{
    internal class File
    {
        private string path { get; set; }
        private string filename { get; set; }
        private Encoding encoding { get; set; }

        public File(string path, string filename, Encoding encoding = null)
        {
            this.path = path;
            this.filename = filename;
            this.encoding = encoding;
        }

        public bool Save<T>(List<T> data)
        {
            Foo<T> foo = this.Read<T>();
            if (foo == null)
                foo = FooHelper.Create<T>();

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
                return null;

            Foo<T> foo = DbRepo.json.DeserializeFromString<Foo<T>>(DbRepo.fileSystem.ReadAllText(path));
            if (foo.data == null)
                foo.data = new List<T>();
            return foo;
        }
    }
}
