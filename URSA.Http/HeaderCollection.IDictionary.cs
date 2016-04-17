using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP header collection.</summary>
    public sealed partial class HeaderCollection : IDictionary<string, string>
    {
        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        ICollection<string> IDictionary<string, string>.Values { get { return _headers.Values.Select(value => value.Value).ToList(); } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        ICollection<string> IDictionary<string, string>.Keys { get { return _headers.Keys; } }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        string IDictionary<string, string>.this[string key]
        {
            get { return _headers[key].Value; }
            set { Set(key, value); }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        void IDictionary<string, string>.Add(string name, string value)
        {
            Add(name, value);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        bool IDictionary<string, string>.ContainsKey(string key)
        {
            return _headers.ContainsKey(key);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            value = null;
            Header header;
            if (!_headers.TryGetValue(key, out header))
            {
                return false;
            }

            value = header.Value;
            return true;
        }
    }
}