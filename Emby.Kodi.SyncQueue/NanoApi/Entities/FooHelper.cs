using System;
using System.Collections.Generic;

namespace NanoApi.Entities
{
    internal static class FooHelper
    {
        public static Foo<T> Create<T>()
        {
            return new Foo<T>
            {
                data = new List<T>()
            };
        }
    }
}
