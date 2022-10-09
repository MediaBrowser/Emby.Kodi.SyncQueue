using NanoApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MediaBrowser.Model.Logging;

namespace NanoApi
{
    public class JsonFile<T> : IDisposable where T : class
    {
        private File file { get; set; }

        public JsonFile(string path, string filename, Encoding encoding, ILogger logger)
        {
            this.file = new File(path, filename, encoding, logger);
        }

        public int Insert(List<T> list)
        {
            Foo<T> foo = this.file.Read<T>();
            if (foo == null)
                foo = new Foo<T>();

            foreach (T current in list)
            {
                foo.data.Add(current);
            }
            this.file.Save<T>(foo);
            return 1;
        }

        public int Delete(Predicate<T> lambda)
        {
            Foo<T> foo = this.file.Read<T>();
            if (foo == null || foo.data.Count <= 0)
                return 0;

            int arg_3D_0 = foo.data.RemoveAll(lambda);
            this.file.Save<T>(foo.data);
            return arg_3D_0;
        }       

        public List<T> Select(Predicate<T> lambda = null)
        {
            Foo<T> foo = this.file.Read<T>();
            if (foo == null)
                return new List<T>();

            if (lambda == null)
                return new List<T>();

            return foo.data.FindAll(lambda);
        }

        public void Dispose()
        {
        }
    }
}
