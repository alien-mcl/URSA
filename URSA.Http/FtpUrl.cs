using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{    
    /// <summary>Represents an FTP/FTPS and SSH url.</summary>
    public class FtpUrl : IpUrl
    {
        private readonly string _asString;
        private readonly string _location;
        private readonly string _path;
        private readonly ParametersCollection _parameters;
        private readonly string[] _segments;
        private readonly int _hashCode;

        internal FtpUrl(
            string url,
            string scheme,
            string userName,
            string password,
            string host,
            ushort port,
            string path,
            ParametersCollection parameters,
            params string[] segments) : base(url, scheme, userName, password, host, (port == 0 ? (scheme == FtpUrlParser.Ssh ? FtpUrlParser.SshPort : (scheme == FtpUrlParser.Ftps ? FtpUrlParser.FtpsPort : FtpUrlParser.FtpPort)) : port))
        {
            port = (port == 0 ? (scheme == FtpUrlParser.Ssh ? FtpUrlParser.SshPort : (scheme == FtpUrlParser.Ftps ? FtpUrlParser.FtpsPort : FtpUrlParser.FtpPort)) : port);
            _path = (!String.IsNullOrEmpty(path) ? path : "/");
            _segments = segments ?? new string[0];
            _parameters = parameters;
            string safePath = (_segments.Length == 0 ? String.Empty : String.Join("/", _segments.Select(segment => UrlParser.ToSafeString(segment, FtpUrlParser.PathAllowedChars))));
            _asString = ToAbsoluteString(
                scheme,
                UrlParser.ToSafeString(userName, UrlParser.LoginAllowed),
                UrlParser.ToSafeString(password, UrlParser.LoginAllowed),
                host,
                port,
                safePath,
                _parameters);
            _location = _asString.Substring(scheme.Length + 1);
            _hashCode = _asString.GetHashCode();
        }

        /// <inheritdoc />
        public override string Location { get { return _location; } }

        /// <inheritdoc />
        public override string Path { get { return _path; } }

        /// <inheritdoc />
        public override string Authority { get { return ToAbsoluteString(Scheme, UserName, Password, Host, Port); } }

        /// <inheritdoc />
        public override ParametersCollection Parameters { get { return _parameters; } }

        /// <summary>Gets a value indicating whether this instance has a query.</summary>
        public bool HasQuery { get { return _parameters != null; } }

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
            FtpUrl other = obj as FtpUrl;
            return (other != null) && (other._asString.Equals(_asString));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <inheritdoc />
        protected override IpUrl CreateInstance(IEnumerable<string> segments, bool? requiresParameters = false)
        {
            StringBuilder path = new StringBuilder(Path.Length * 2);
            IList<string> newSegments = new List<string>(_segments.Length + 1);
            foreach (string segment in segments)
            {
                path.Append("/").Append(UrlParser.ToSafeString(segment, FtpUrlParser.PathAllowedChars));
                newSegments.Add(segment);
            }

            var parameters = (!requiresParameters.HasValue ? null :
                (requiresParameters.Value ? (Parameters != null ? Parameters.DeepCopy() : new ParametersCollection(";", "=")) : Parameters));
            string url = ToAbsoluteString(Scheme, UserName, Password, Host, Port, path.ToString(), parameters);
            return new FtpUrl(url, Scheme, UserName, Password, Host, Port, path.ToString(), Parameters, segments.ToArray());
        }

        private static string ToAbsoluteString(string scheme, string userName, string password, string host, uint port, string path = null, ParametersCollection parameters = null)
        {
            string portString = ((scheme == FtpUrlParser.Ssh) && (port == FtpUrlParser.SshPort)) || 
                ((scheme == FtpUrlParser.Ftps) && (port == FtpUrlParser.FtpsPort)) || 
                (port == FtpUrlParser.FtpPort) ? String.Empty : ":" + port;
            string passwordString = (!String.IsNullOrEmpty(password) ? String.Format(":{0}@", password) : (!String.IsNullOrEmpty(userName) ? "@" : String.Empty));
            if (path == null)
            {
                return String.Format("{0}://{1}{2}{3}{4}/", scheme, userName, passwordString, host, portString);
            }

            return String.Format(
                "{0}://{1}{2}{3}{4}/{5}{6}",
                scheme,
                userName,
                (!String.IsNullOrEmpty(password) ? String.Format(":{0}@", password) : (!String.IsNullOrEmpty(userName) ? "@" : String.Empty)),
                host,
                portString,
                path,
                (parameters != null ? String.Format(";{0}", parameters.ToString(FtpUrlParser.PathAllowedChars)) : String.Empty));
        }
    }
}