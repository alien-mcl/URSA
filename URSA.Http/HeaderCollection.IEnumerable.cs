using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP header collection.</summary>
    public sealed partial class HeaderCollection : IEnumerable<Header>
    {
        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public IEnumerator<Header> GetEnumerator()
        {
            return _headers.Values.GetEnumerator();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return _headers.Select(header => new KeyValuePair<string, string>(header.Key, header.Value.Value)).GetEnumerator();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}