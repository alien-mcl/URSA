using System.Collections.Generic;

namespace System.Collections.Generic
{
    internal static class CollectionExtensions
    {
        internal static T AddAndReturn<T>(this ICollection<T> collection, T item)
        {
            collection.Add(item);
            return item;
        }
    }
}