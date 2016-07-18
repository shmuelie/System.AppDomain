using System.Collections.Generic;

namespace System
{
    internal static class CollectionHelpers
    {
        public static IEnumerable<T> Collect<T>(this T @this)
        {
            yield return @this;
        }

        public static IEnumerable<T> AndThis<T>(this IEnumerable<T> @this, T also)
        {
#pragma warning disable HeapAnalyzerEnumeratorAllocationRule // Possible allocation of reference type enumerator
            foreach (T t in @this)
#pragma warning restore HeapAnalyzerEnumeratorAllocationRule // Possible allocation of reference type enumerator
            {
                yield return t;
            }
            yield return also;
        }
    }
}
