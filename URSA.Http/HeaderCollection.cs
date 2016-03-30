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
        internal const string Token = "[^\x00-\x20'()<>@,;:\\\"/\\[\\]?={}]+";
        internal static readonly Regex Header = new Regex(String.Format("(?<Name>{0}):(?<Value>(.|(?<=\r)\n(?=[ \t]))*)", Token));
        private readonly IDictionary<string, Header> _headers = new Dictionary<string, Header>(Http.Header.Comparer);

        /// <summary>Initializes a new instance of the <see cref="HeaderCollection"/> class.</summary>
        public HeaderCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HeaderCollection"/> class.</summary>
        /// <param name="headers">The headers.</param>
        public HeaderCollection(params Header[] headers)
        {
            if (headers != null)
            {
                headers.ForEach(header => Add(header));
            }
        }

        /// <summary>Gets or sets the header by it's name.</summary>
        /// <param name="name">Name of the header.</param>
        /// <returns>Instance of the <see cref="Header" /> if the header with given <paramref name="name" /> exists; otherwise <b>null</b>.</returns>
        public Header this[string name]
        {
            get
            {
                Header result;
                if (_headers.TryGetValue(name, out result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                if (name.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("name");
                }

                if ((value != null) && (!Http.Header.Comparer.Equals(name, value.Name)))
                {
                    throw new InvalidOperationException(String.Format("Header name '{0}' and actual header '{1}' mismatch.", name, value.Name));
                }

                Remove(name);
                if (value != null)
                {
                    Set(value);
                }
            }
        }

        /// <summary>Tries to parse a given string as a <see cref="HeaderCollection" />.</summary>
        /// <param name="headers">String to be parsed.</param>
        /// <param name="headersCollection">Resulting collection of headers if parsing was successful; otherwise <b>null</b>.</param>
        /// <returns><b>true</b> if the parsing was successful; otherwise <b>false</b>.</returns>
        public static bool TryParse(string headers, out HeaderCollection headersCollection)
        {
            bool result = false;
            headersCollection = null;
            try
            {
                headersCollection = Parse(headers);
                result = true;
            }
            catch
            {
            }

            return result;
        }

        /// <summary>Parses a given string as a <see cref="HeaderCollection" />.</summary>
        /// <param name="headers">String to be parsed.</param>
        /// <returns>Instance of the <see cref="HeaderCollection" />.</returns>
        public static HeaderCollection Parse(string headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            if (headers.Length == 0)
            {
                throw new ArgumentOutOfRangeException("headers");
            }

            HeaderCollection result = new HeaderCollection();
            var matches = Header.Matches(headers);
            foreach (Match match in matches)
            {
                result.Add(Http.Header.Parse(match.Value));
            }

            return result;
        }

        /// <summary>Adds a new header.</summary>
        /// <remarks>If the header already exists, method will merge new value and parameters with the existing ones.</remarks>
        /// <param name="header">Header to be added.</param>
        /// <returns>Instance of the <see cref="Header" /> either updated or added.</returns>
        public Header Add(Header header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            Header result;
            if (!_headers.TryGetValue(header.Name, out result))
            {
                _headers[header.Name] = result = header;
            }
            else
            {
                header.Values.ForEach(value => result.Values.AddUnique(value));
            }

            return result;
        }

        /// <summary>Adds a new header.</summary>
        /// <remarks>If the header already exists, method will merge new value and parameters with the existing ones.</remarks>
        /// <param name="name">Name of the header to be added.</param>
        /// <param name="value">Value of the header to be added.</param>
        /// <returns>Instance of the <see cref="Header" /> either updated or added.</returns>
        public Header Add(string name, string value)
        {
            return Add(Http.Header.Parse(String.Format("{0}:{1}", name, value)));
        }

        /// <summary>Sets a new header.</summary>
        /// <remarks>If the header already exists, it is replaced with the new one.</remarks>
        /// <param name="header">Header to be set.</param>
        /// <returns>Instance of the <see cref="Header" /> set.</returns>
        public Header Set(Header header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            return _headers[header.Name] = header;
        }

        /// <summary>Sets a new header.</summary>
        /// <remarks>If the header already exists, it is replaced with the new one.</remarks>
        /// <param name="name">Name of the header to be added.</param>
        /// <param name="value">Value of the header to be added.</param>
        /// <returns>Instance of the <see cref="Header" /> set.</returns>
        public Header Set(string name, string value)
        {
            return Set(Http.Header.Parse(String.Format("{0}:{1}", name, value)));
        }

        /// <summary>Removes the header with given name.</summary>
        /// <param name="name">Name of the header to be removed.</param>
        /// <returns><b>true</b> if the header was removed; otherwise <b>false</b>.</returns>
        public bool Remove(string name)
        {
            return _headers.Remove(name);
        }

        /// <summary>Merges headers.</summary>
        /// <param name="headers">Headers to be merged.</param>
        public void Merge(HeaderCollection headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                Add(header);
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return String.Format("{0}\r\n\r\n", String.Join("\r\n", _headers.Values));
        }
    }
}