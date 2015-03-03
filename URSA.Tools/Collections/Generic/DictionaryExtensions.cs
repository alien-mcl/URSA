using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    /// <summary>Provides useful <see cref="IDictionary{TKey, TValue}" /> extensions.</summary>
    public static class DictionaryExtensions
    {
        /// <summary>Gets the element under the <paramref name="key" />. If the item doesn't exist, it uses <paramref name="defaultValue" /> to create it.</summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value under <paramref name="key" />.</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            if ((!typeof(TKey).IsValueType) && ((object)key == null))
            {
                throw new ArgumentOutOfRangeException("key");
            }

            if (defaultValue == null)
            {
                throw new ArgumentNullException("defaultValue");
            }

            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = dictionary[key] = defaultValue();
            }

            return result;
        }
    }
}