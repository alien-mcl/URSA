using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Linq
{
    /// <summary>Provides useful <see cref="IEnumerable{T}" /> extension methods.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>Applies the given <paramref name="action" /> on each of the <paramref name="items" />.</summary>
        /// <param name="items">Items to be iterated through.</param>
        /// <param name="action">Action to be applied.</param>
        [ExcludeFromCodeCoverage]
        public static void ForEach(this IEnumerable items, Action<object> action)
        {
            if ((items == null) || (action == null))
            {
                return;
            }

            foreach (var item in items)
            {
                action(item);
            }
        }

        /// <summary>Applies the given <paramref name="action" /> on each of the <paramref name="items" />.</summary>
        /// <param name="items">Items to be iterated through.</param>
        /// <param name="action">Action to be applied.</param>
        [ExcludeFromCodeCoverage]
        public static void ForEach(this IEnumerable items, Action<object, int> action)
        {
            if ((items == null) || (action == null))
            {
                return;
            }

            int index = 0;
            foreach (var item in items)
            {
                action(item, index);
                index++;
            }
        }

        /// <summary>Applies the given <paramref name="action" /> on each of the <paramref name="items" />.</summary>
        /// <typeparam name="T">Type of items in the enumeration.</typeparam>
        /// <param name="items">Items to be iterated through.</param>
        /// <param name="action">Action to be applied.</param>
        [ExcludeFromCodeCoverage]
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if ((items == null) || (action == null))
            {
                return;
            }

            foreach (T item in items)
            {
                action(item);
            }
        }

        /// <summary>Applies the given <paramref name="action" /> on each of the <paramref name="items" />.</summary>
        /// <typeparam name="T">Type of items in the enumeration.</typeparam>
        /// <param name="items">Items to be iterated through.</param>
        /// <param name="action">Action to be applied.</param>
        [ExcludeFromCodeCoverage]
        public static void ForEach<T>(this IEnumerable<T> items, Action<T, int> action)
        {
            if ((items == null) || (action == null))
            {
                return;
            }

            int index = 0;
            foreach (T item in items)
            {
                action(item, index);
                index++;
            }
        }

        /// <summary>Adds a new item to the collection if it does not exist.</summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Collection to add to.</param>
        /// <param name="newItem">Item to be added.</param>
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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

        /// <summary>Adds a range of items to the collection.</summary>
        /// <typeparam name="T">Type of items in the collection.</typeparam>
        /// <param name="items">Collection to add to.</param>
        /// <param name="newItems">Items to be added.</param>
        [ExcludeFromCodeCoverage]
        public static void AddRange<T>(this ICollection<T> items, IEnumerable<T> newItems)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }

            foreach (var item in newItems)
            {
                items.Add(item);
            }
        }
    }
}