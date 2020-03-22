using System;
using System.Collections.Generic;

namespace NanoApi.Entities
{
    internal class Foo<T>
    {
        public List<T> data { get; set; }
    }
}
