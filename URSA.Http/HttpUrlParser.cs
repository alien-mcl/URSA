using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Provides an HTTP and HTTPS URL parsing facility.</summary>
    public class HttpUrlParser : IpUrlParser
    {
        internal const ushort HttpPort = 80;
        internal const ushort HttpsPort = 443;
        internal const string Http = "http";
        internal const string Https = "https";
        internal static readonly char[] PathReserved = { ';', '/', '?' };
        internal static readonly char[] PathAllowedChars = UChar.Concat(Reserved.Except(PathReserved)).ToArray();
        private static readonly string[] Schemes = { Http, Https };

        private readonly ICollection<string> _segments = new List<string>();
        private ParametersCollection _query;
        private string _fragment;

        /// <inheritdoc />
        public override IEnumerable<string> SupportedSchemes { get { return Schemes; } }

        /// <inheritdoc />
        public override bool AllowsRelativeAddresses { get { return true; } }

        /// <inheritdoc />
        protected override ushort DefaultPort { get { return (Scheme == Https ? HttpsPort : HttpPort); } }

        /// <inheritdoc />
        protected override char[] PathAllowed { get { return PathAllowedChars; } }

        /// <inheritdoc />
        protected override bool SupportsLogin { get { return false; } }

        /// <inheritdoc />
        protected override void ParsePath(StringBuilder actualUrl, int index)
        {
            int lastDelimiter = index;
            int lastSegment = index;
            index++;
            for (; index < actualUrl.Length; index++)
            {
                switch (actualUrl[index])
                {
                    case '/':
                        if (index - lastSegment - 1 > 0)
                        {
                            _segments.Add(actualUrl.ToString(lastSegment + 1, index - lastSegment - 1));
                        }

                        lastSegment = index;
                        break;
                    case '%':
                        index = DecodeEscape(actualUrl, index);
                        break;
                    case '#':
                        if (index - lastSegment - 1 > 0)
                        {
                            _segments.Add(actualUrl.ToString(lastSegment + 1, index - lastSegment - 1));
                        }

                        Path = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                        ParseFragment(actualUrl, index);
                        return;
                    case '?':
                        if (index - lastSegment - 1 > 0)
                        {
                            _segments.Add(actualUrl.ToString(lastSegment + 1, index - lastSegment - 1));
                        }

                        Path = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                        _query = new ParametersCollection("&", "=");
                        ParseQuery(actualUrl, index);
                        return;
                }
            }

            if (lastSegment + 1 != index)
            {
                _segments.Add(actualUrl.ToString(lastSegment + 1, index - lastSegment - 1));
            }

            Path = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
        }

        /// <inheritdoc />
        protected override IpUrl CreateInstance(string url)
        {
            return new HttpUrl(Scheme.Length > 0, url, Scheme, Host, Port, Path, _query, _fragment, _segments.ToArray());
        }

        private void ParseQuery(StringBuilder actualUrl, int index)
        {
            int lastDelimiter = index;
            index++;
            string currentKey = null;
            for (; index < actualUrl.Length; index++)
            {
                switch (actualUrl[index])
                {
                    case '&':
                        if (currentKey == null)
                        {
                            lastDelimiter = index;
                            break;
                        }

                        _query.AddValue(currentKey, actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1));
                        lastDelimiter = index;
                        currentKey = null;
                        break;
                    case '=':
                        currentKey = actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1);
                        lastDelimiter = index;
                        break;
                    case '%':
                        index = DecodeEscape(actualUrl, index);
                        break;
                    case '#':
                        _query.AddValue(currentKey, actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1));
                        ParseFragment(actualUrl, index);
                        return;
                }
            }

            if (currentKey != null)
            {
                _query.AddValue(currentKey, actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1));
            }
            else
            {
                _query.AddValue(actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1), String.Empty);
            }
        }

        private void ParseFragment(StringBuilder actualUrl, int index)
        {
            int lastDelimiter = index;
            index++;
            for (; index < actualUrl.Length; index++)
            {
                switch (actualUrl[index])
                {
                    case '%':
                        index = DecodeEscape(actualUrl, index);
                        break;
                }
            }

            _fragment = actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1);
        }
    }
}