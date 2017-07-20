using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if CORE
using Microsoft.AspNetCore.WebUtilities;
#else
using System.Web;
#endif

namespace URSA.Web.Http
{
    /// <summary>Defines possible existing fragment behaviors when adding a new one.</summary>
    public enum ExistingFragment
    {
        /// <summary>Instructs to throw an exception when there is already a fragment and the new one is not an <b>null</b>.</summary>
        Throw,

        /// <summary>Instructs to replace existing fragment with a new one.</summary>
        Replace,

        /// <summary>Instructs to append a new fragment to the existing one.</summary>
        Append,

        /// <summary>Instructs to move an existing fragment to path as another segment.</summary>
        MoveAsSegment
    }

    /// <summary>Represents an HTTP/HTTPS url.</summary>
    public class HttpUrl : IpUrl
    {
        private readonly bool _isAbsolute;
        private readonly string _asString;
        private readonly string _location;
        private readonly string _path;
        private readonly ParametersCollection _query;
        private readonly string _fragment;
        private readonly string[] _segments;
        private readonly int _hashCode;

        internal HttpUrl(
            bool isAbsolute,
            string url,
            string scheme,
            string host,
            ushort port,
            string path,
            ParametersCollection query,
            string fragment,
            params string[] segments) : base(url, scheme, null, null, host, (port == 0 ? (scheme == HttpUrlParser.Https ? HttpUrlParser.HttpsPort : HttpUrlParser.HttpPort) : port))
        {
            port = (port == 0 ? (scheme == HttpUrlParser.Https ? HttpUrlParser.HttpsPort : HttpUrlParser.HttpPort) : port);
            _isAbsolute = isAbsolute;
            _path = (!String.IsNullOrEmpty(path) ? path : "/");
            _fragment = fragment;
            _segments = segments ?? new string[0];
            _query = query;
            string safePath = (_segments.Length == 0 ? String.Empty : String.Join("/", _segments.Select(segment => UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars))));
            if (isAbsolute)
            {
                _location = ToAbsoluteString(scheme, host, port, _path.TrimStart('/'), _query, _fragment).Substring(scheme.Length + 1);
                _asString = ToAbsoluteString(scheme, host, port, safePath, _query, _fragment);
            }
            else
            {
                _location = _asString = ToRelativeString("/" + safePath, _query, _fragment);
            }

            _hashCode = _asString.GetHashCode();
        }

        /// <inheritdoc />
        public override bool IsAbsolute { get { return _isAbsolute; } }

        /// <inheritdoc />
        public override string Location { get { return _location; } }

        /// <inheritdoc />
        public override string Path { get { return _path; } }

        /// <inheritdoc />
        public override string Authority { get { return ToAbsoluteString(Scheme, Host, Port); } }

        /// <summary>Gets a relative version of this URL.</summary>
        public HttpUrl AsRelative
        {
            get
            {
                return (!_isAbsolute ? this : new HttpUrl(false, ToRelativeString(Path, _query, _fragment), Scheme, Host, Port, Path, _query, _fragment, _segments));
            }
        }

        /// <inheritdoc />
        public override ParametersCollection Parameters { get { return _query; } }

        /// <summary>Gets the query string parameters.</summary>
        public ParametersCollection Query { get { return _query; } }

        /// <summary>Gets a value indicating whether this instance has a query.</summary>
        public bool HasQuery { get { return _query != null; } }

        /// <summary>Gets the fragment (a.k.a. hash).</summary>
        public string Fragment { get { return _fragment; } }

        /// <summary>Gets a value indicating whether this instance has fragment.</summary>
        public bool HasFragment { get { return _fragment != null; } }

        /// <inheritdoc />
        public override IEnumerable<string> Segments { get { return _segments; } }

        /// <summary>Converts a given <paramref name="uri" /> into an HTTP URL.</summary>
        /// <param name="uri">The Uri to be converted.</param>
        /// <returns>HTTP URL or <b>null</b> if the passed <paramref name="uri" /> was also null.</returns>
        public static explicit operator HttpUrl(Uri uri)
        {
            if (Equals(uri, null))
            {
                return null;
            }

            ParametersCollection query = null;
            if (uri.IsAbsoluteUri)
            {
                if (!HttpUrlParser.Schemes.Contains(uri.Scheme))
                {
                    throw new InvalidOperationException(String.Format("Cannot convert Uri of scheme '{0}' to HTTP URL.", uri.Scheme));
                }

                query = (uri.Query.Length > 0 ? ParseQueryString(uri.Query) : null);
                return new HttpUrl(true, uri.ToString(), uri.Scheme, uri.Host, (ushort)uri.Port, uri.AbsolutePath, query, (uri.Fragment.Length > 0 ? uri.Fragment : null));
            }

            string url = uri.ToString();
            string path = url;
            string queryString = null;
            string fragment = null;
            int indexOf;
            if ((indexOf = url.IndexOf('?')) != -1)
            {
                path = url.Substring(0, indexOf);
                if ((indexOf = (queryString = url.Substring(indexOf)).IndexOf('#')) != -1)
                {
                    fragment = queryString.Substring(indexOf);
                    queryString = queryString.Substring(0, indexOf);
                }
            }
            else if ((indexOf = url.IndexOf('#')) != -1)
            {
                path = url.Substring(0, indexOf);
                fragment = url.Substring(indexOf);
            }

            if (fragment == "#")
            {
                fragment = String.Empty;
            }

            if (queryString != null)
            {
                query = ParseQueryString(queryString);
            }

            return new HttpUrl(false, url, null, null, 0, path, query, fragment);
        }

        /// <summary>Adds two URLs.</summary>
        /// <remarks>One of the operands should be a relative URL.</remarks>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <returns>New url being a combination of the two operands.</returns>
        public static HttpUrl operator +(HttpUrl leftOperand, HttpUrl rightOperand)
        {
            if ((Equals(leftOperand, null)) && (Equals(rightOperand, null)))
            {
                return null;
            }

            if ((!leftOperand.IsAbsolute) && (!rightOperand.IsAbsolute))
            {
                throw new InvalidOperationException("Cannot concatenate two relative URLs.");
            }

            if ((leftOperand.HasFragment) && (rightOperand.HasFragment))
            {
                throw new InvalidOperationException("Both URLs cannot have a fragment.");
            }

            HttpUrl baseUrl;
            HttpUrl relativeUrl;
            if (leftOperand.IsAbsolute)
            {
                baseUrl = leftOperand;
                relativeUrl = rightOperand;
            }
            else
            {
                baseUrl = rightOperand;
                relativeUrl = leftOperand;
            }

            var segments = new List<string>(baseUrl._segments);
            segments.AddRange(relativeUrl._segments);
            string fragment = baseUrl.Fragment ?? relativeUrl.Fragment;
            var query = baseUrl._query ?? relativeUrl._query;
            if ((baseUrl._query != null) && (relativeUrl._query != null))
            {
                relativeUrl._query.ForEach(parameter => query.AddValue(parameter.Key, parameter.Value));
            }

            string path = String.Join("/", segments);
            string url = ToAbsoluteString(baseUrl.Scheme, baseUrl.Host, baseUrl.Port, path, query, fragment);
            return new HttpUrl(baseUrl._isAbsolute, url, baseUrl.Scheme, baseUrl.Host, baseUrl.Port, "/" + path, query, fragment, segments.ToArray());
        }

        /// <summary>Adds a base url and a relative url string.</summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <returns>New url being a combination of the two operands.</returns>
        public static HttpUrl operator +(HttpUrl leftOperand, string rightOperand)
        {
            if ((Equals(leftOperand, null)) && (Equals(rightOperand, null)))
            {
                return null;
            }

            if (!leftOperand.IsAbsolute)
            {
                throw new InvalidOperationException("Url must be an absolute one.");
            }

            if (String.IsNullOrEmpty(rightOperand))
            {
                return leftOperand;
            }

            if (!rightOperand.StartsWith("/"))
            {
                rightOperand = "/" + rightOperand;
            }

            Url url = UrlParser.Parse(rightOperand);
            HttpUrl relativeUrl = url as HttpUrl;
            if (relativeUrl == null)
            {
                throw new InvalidOperationException(String.Format("Cannot add URLs of type '{0}' and '{1}'.", leftOperand.GetType(), relativeUrl.GetType()));
            }

            return leftOperand + relativeUrl;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _asString;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            HttpUrl other = obj as HttpUrl;
            return (other != null) && (other._asString.Equals(_asString));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>Withes the fragment.</summary>
        /// <remarks>Depending on the existing fragment and the <paramref name="fragment" /> being set, output may differ.
        /// Setting a <b>null</b> value will remove the fragment regardless the <paramref name="handledAs" /> setting.</remarks>
        /// <param name="fragment">The fragment.</param>
        /// <param name="handledAs">Instructs on how to behave when an URL has already a fragment.</param>
        /// <returns>New URL with fragment modified.</returns>
        public HttpUrl WithFragment(string fragment, ExistingFragment handledAs = ExistingFragment.Throw)
        {
            if (!HasFragment)
            {
                return (fragment == null ? this : new HttpUrl(_isAbsolute, OriginalUrl + "#" + fragment, Scheme, Host, Port, Path, _query, fragment, _segments));
            }

            if (fragment == null)
            {
                return new HttpUrl(_isAbsolute, OriginalUrl.Substring(0, OriginalUrl.IndexOf('#')), Scheme, Host, Port, Path, _query, null, _segments);
            }

            switch (handledAs)
            {
                case ExistingFragment.Append:
                    return new HttpUrl(_isAbsolute, OriginalUrl + fragment, Scheme, Host, Port, Path, _query, Fragment + fragment, _segments);
                case ExistingFragment.Replace:
                    return new HttpUrl(_isAbsolute, OriginalUrl.Substring(0, OriginalUrl.IndexOf('#') + 1) + fragment, Scheme, Host, Port, Path, _query, fragment, _segments);
                case ExistingFragment.MoveAsSegment:
                    return ((HttpUrl)AddSegment(Fragment)).WithFragment(fragment, ExistingFragment.Replace);
            }

            throw new InvalidOperationException("Url has already a fragment.");
        }

        /// <inheritdoc />
        protected override IpUrl CreateInstance(IEnumerable<string> segments, bool? requiresParameters = false)
        {
            StringBuilder path = new StringBuilder(Path.Length * 2);
            IList<string> newSegments = new List<string>(_segments.Length + 1);
            foreach (string segment in segments)
            {
                path.Append("/").Append(UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars));
                newSegments.Add(segment);
            }

            string url = (_isAbsolute ? ToAbsoluteString(Scheme, Host, Port, path.ToString(), _query, _fragment) : ToRelativeString(path.ToString(), _query, _fragment));
            var query = (!requiresParameters.HasValue ? null :
                (requiresParameters.Value ? (Query != null ? Query.DeepCopy() : new ParametersCollection("&", "=")) : Query));
            return new HttpUrl(_isAbsolute, url, Scheme, Host, Port, path.ToString(), query, Fragment, segments.ToArray());
        }

        private static ParametersCollection ParseQueryString(string queryString)
        {
#if CORE
            return (ParametersCollection)QueryHelpers.ParseQuery(queryString);
#else
            return (ParametersCollection)HttpUtility.ParseQueryString(queryString);
#endif
        }

        private static string ToAbsoluteString(string scheme, string host, ushort port, string path = null, ParametersCollection query = null, string fragment = null)
        {
            string portString = (((scheme == HttpUrlParser.Https) && (port == HttpUrlParser.HttpsPort)) || (port == HttpUrlParser.HttpPort) ? String.Empty : ":" + port);
            if (path == null)
            {
                return String.Format("{0}://{1}{2}/", scheme, host, portString);
            }

            return String.Format(
                "{0}://{1}{2}/{3}{4}{5}",
                scheme,
                host,
                portString,
                path,
                (query != null ? String.Format("?{0}", query.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty),
                (fragment != null ? String.Format("#{0}", UrlParser.ToSafeString(fragment, HttpUrlParser.PathAllowedChars)) : String.Empty));
        }

        private static string ToRelativeString(string path, ParametersCollection query = null, string fragment = null)
        {
            if (path.Length == 0)
            {
                path = "/";
            }

            return String.Format(
                "{0}{1}{2}",
                path,
                (query != null ? String.Format("?{0}", query.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty),
                (fragment != null ? String.Format("#{0}", UrlParser.ToSafeString(fragment, HttpUrlParser.PathAllowedChars)) : String.Empty));
        }
    }
}