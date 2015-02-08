using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>Provides useful <see cref="IEnumerable{T}" /> extension methods.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>Applies the given <paramref name="action" /> on each of the <see cref="items" />.</summary>
        /// <typeparam name="T">Type of items in the enumeration.</typeparam>
        /// <param name="items">Items to be iterated through.</param>
        /// <param name="action">Action to be applied.</param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if ((items != null) && (action != null))
            {
                foreach (T item in items)
                {
                    action(item);
                }
            }
        }

        /// <summary>Adds a new item to the collection if it does not exist.</summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Collection to add to.</param>
        /// <param name="newItem">Item to be added.</param>
        public static void AddUnique<T>(this ICollection<T> items, T newItem)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (newItem == null)
            {
                throw new ArgumentNullException("newItem");
            }

            if (!items.Contains(newItem))
            {
                items.Add(newItem);
            }
        }

        /// <summary>Adds a new item to the collection if it does not exist.</summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Collection to add to.</param>
        /// <param name="newItem">Item to be added.</param>
        /// <param name="comparer">Comparer to be used when comparing items.</param>
        public static void AddUnique<T>(this ICollection<T> items, T newItem, IEqualityComparer<T> comparer)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (newItem == null)
            {
                throw new ArgumentNullException("newItem");
            }

            if (!items.Contains(newItem, comparer))
            {
                items.Add(newItem);
            }
        }
    }
}