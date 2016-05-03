using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            if (isAbsolute)
            {
                string portString = ((scheme == HttpUrlParser.Https) && (port == HttpUrlParser.HttpsPort)) || (port == HttpUrlParser.HttpPort) ? String.Empty : ":" + port;
                _location = String.Format(
                    "//{0}{1}{2}{3}{4}",
                    host,
                    portString,
                    _path,
                    (_query != null ? String.Format("?{0}", _query) : String.Empty),
                    (_fragment != null ? String.Format("#{0}", _fragment) : String.Empty));
                _asString = String.Format(
                    "{0}://{1}{2}/{3}{4}{5}",
                    scheme,
                    host,
                    portString,
                    (_segments.Length > 0 ? String.Join("/", _segments.Select(segment => UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars))) : String.Empty),
                    (_query != null ? String.Format("?{0}", _query.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty),
                    (_fragment != null ? String.Format("#{0}", UrlParser.ToSafeString(_fragment, HttpUrlParser.PathAllowedChars)) : String.Empty));
            }
            else
            {
                _location = String.Format(
                    "{0}{1}{2}",
                    _path,
                    (_query != null ? String.Format("?{0}", _query) : String.Empty),
                    (_fragment != null ? String.Format("#{0}", _fragment) : String.Empty));
                _asString = String.Format(
                    "{0}{1}{2}",
                    (_segments.Length > 0 ? String.Join("/", _segments.Select(segment => UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars))) : String.Empty),
                    (_query != null ? String.Format("?{0}", _query.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty),
                    (_fragment != null ? String.Format("#{0}", UrlParser.ToSafeString(_fragment, HttpUrlParser.PathAllowedChars)) : String.Empty));
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
        public override ParametersCollection Parameters { get { return _query; } }

        /// <inheritdoc />
        public ParametersCollection Query { get { return _query; } }

        /// <summary>Gets a value indicating whether this instance has a query.</summary>
        public bool HasQuery { get { return _query != null; } }

        /// <inheritdoc />
        public string Fragment { get { return _fragment ?? String.Empty; } }

        /// <summary>Gets a value indicating whether this instance has fragment.</summary>
        public bool HasFragment { get { return _fragment != null; } }

        /// <inheritdoc />
        public override IEnumerable<string> Segments { get { return _segments; } }

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
                return (fragment == null ? this : new HttpUrl(_isAbsolute, OriginalUrl, Scheme, Host, Port, Path, _query, fragment, _segments));
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
        protected override IpUrl CreateInstance(IEnumerable<string> segments)
        {
            StringBuilder path = new StringBuilder(Path.Length * 2);
            IList<string> newSegments = new List<string>(_segments.Length + 1);
            foreach (string segment in segments)
            {
                path.Append("/").Append(UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars));
                newSegments.Add(segment);
            }

            string url;
            if (_isAbsolute)
            {
                string portString = ((Scheme == HttpUrlParser.Https) && (Port == HttpUrlParser.HttpsPort)) || (Port == HttpUrlParser.HttpPort) ? String.Empty : ":" + Port;
                url = String.Format(
                    "{0}://{1}{2}/{3}{4}{5}",
                    Scheme,
                    Host,
                    portString,
                    path,
                    (_query != null ? String.Format("?{0}", _query.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty),
                    (_fragment != null ? String.Format("#{0}", UrlParser.ToSafeString(_fragment, HttpUrlParser.PathAllowedChars)) : String.Empty));
            }
            else
            {
                url = String.Format(
                    "{0}{1}{2}",
                    path,
                    (_query != null ? String.Format("?{0}", _query.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty),
                    (_fragment != null ? String.Format("#{0}", UrlParser.ToSafeString(_fragment, HttpUrlParser.PathAllowedChars)) : String.Empty));
            }

            return new HttpUrl(_isAbsolute, url, Scheme, Host, Port, path.ToString(), Query, Fragment, segments.ToArray());
        }
    }
}