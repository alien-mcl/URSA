using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP header collection.</summary>
    public sealed partial class HeaderCollection
    {
        /// <summary>Gets the count if headers.</summary>
        [ExcludeFromCodeCoverage]
        public int Count { get { return _headers.Count; } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        bool ICollection<KeyValuePair<string, string>>.IsReadOnly { get { return _headers.IsReadOnly; } }

        /// <summary>Clears the collection.</summary>
        [ExcludeFromCodeCoverage]
        public void Clear()
        {
            _headers.Clear();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            Set(item.Key, item.Value);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            _headers.Clear();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            Header header = this[item.Key];
            return (header != null) && (Http.Header.Comparer.Equals(header.Value, item.Value));
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            _headers.Select(header => new KeyValuePair<string, string>(header.Key, header.Value.Value)).ToArray().CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            Header header = this[item.Key];
            if (header == null)
            {
                return false;
            }

            HeaderValue value = header.Values.FirstOrDefault(valueItem => valueItem.Value == item.Value);
            if (value == null)
            {
                return false;
            }

            header.Values.Remove(value);
            return true;
        }
    }
}