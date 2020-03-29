using System;
using System.Collections.Generic;

namespace Varbsorb
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Tap<T>(this IEnumerable<T> enumerable, Action<T> fn)
        {
            foreach (var value in enumerable)
            {
                fn(value);
                yield return value;
            }
        }
    }
}