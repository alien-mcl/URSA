using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RomanticWeb.Linq.Model;

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
            string portString = ((scheme == FtpUrlParser.Ssh) && (port == FtpUrlParser.SshPort)) || ((scheme == FtpUrlParser.Ftps) && (port == FtpUrlParser.FtpsPort)) || (port == FtpUrlParser.FtpPort) ? String.Empty : ":" + port;
            _location = String.Format(
                "//{0}{1}{2}{3}{4}{5}",
                userName,
                (!String.IsNullOrEmpty(password) ? String.Format(":{0}@", password) : (!String.IsNullOrEmpty(userName) ? "@" : String.Empty)),
                host,
                portString,
                _path,
                (_parameters != null ? String.Format(";{0}", _parameters) : String.Empty));
            _asString = String.Format(
                "{0}://{1}{2}{3}{4}/{5}{6}",
                scheme,
                UrlParser.ToSafeString(userName, UrlParser.LoginAllowed),
                (!String.IsNullOrEmpty(password) ? String.Format(":{0}@", UrlParser.ToSafeString(password, UrlParser.LoginAllowed)) : (!String.IsNullOrEmpty(userName) ? "@" : String.Empty)),
                host,
                portString,
                (_segments.Length > 0 ? String.Join("/", _segments.Select(segment => UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars))) : String.Empty),
                (_parameters != null ? String.Format(";{0}", _parameters.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty));
            _hashCode = _asString.GetHashCode();
        }

        /// <inheritdoc />
        public override string Location { get { return _location; } }

        /// <inheritdoc />
        public override string Path { get { return _path; } }

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
                path.Append("/").Append(UrlParser.ToSafeString(segment, HttpUrlParser.PathAllowedChars));
                newSegments.Add(segment);
            }

            string portString = ((Scheme == HttpUrlParser.Https) && (Port == HttpUrlParser.HttpsPort)) || (Port == HttpUrlParser.HttpPort) ? String.Empty : ":" + Port;
            var parameters = (!requiresParameters.HasValue ? null :
                (requiresParameters.Value ? (Parameters != null ? Parameters.Clone() : new ParametersCollection(";", "=")) : Parameters));
            string url = String.Format(
                "{0}://{1}{2}{3}{4}/{5}{6}",
                Scheme,
                UserName,
                (!String.IsNullOrEmpty(Password) ? String.Format(":{0}@", Password) : (!String.IsNullOrEmpty(UserName) ? "@" : String.Empty)),
                Host,
                portString,
                path,
                (parameters != null ? String.Format(";{0}", parameters.ToString(HttpUrlParser.PathAllowedChars)) : String.Empty));
            return new FtpUrl(url, Scheme, UserName, Password, Host, Port, path.ToString(), Parameters, segments.ToArray());
        }
    }
}
