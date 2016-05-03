using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Provides an FTP/FTPS and SSH URL parsing facility.</summary>
    public class FtpUrlParser : IpUrlParser
    {
        internal const ushort FtpPort = 21;
        internal const ushort FtpsPort = 990;
        internal const ushort SshPort = 22;
        internal const string Ftp = "ftp";
        internal const string Ftps = "ftps";
        internal const string Ssh = "ssh";
        internal static readonly char[] PathReserved = { ';', '/', '?' };
        internal static readonly char[] PathAllowedChars = UChar.Concat(Reserved.Except(PathReserved)).ToArray();
        private static readonly string[] Schemes = { Ftp, Ftps, Ssh };

        private readonly ICollection<string> _segments = new List<string>();
        private ParametersCollection _parameters;

        /// <inheritdoc />
        public override IEnumerable<string> SupportedSchemes { get { return Schemes; } }

        /// <inheritdoc />
        public override bool AllowsRelativeAddresses { get { return false; } }

        /// <inheritdoc />
        protected override ushort DefaultPort { get { return (Scheme == Ssh ? SshPort : (Scheme == Ftps ? FtpsPort : FtpPort)); } }

        /// <inheritdoc />
        protected override char[] PathAllowed { get { return PathAllowedChars; } }

        /// <inheritdoc />
        protected override bool SupportsLogin { get { return true; } }

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
                    case ';':
                        if (index - lastSegment - 1 > 0)
                        {
                            _segments.Add(actualUrl.ToString(lastSegment + 1, index - lastSegment - 1));
                        }

                        Path = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                        _parameters = new ParametersCollection(";", "=");
                        ParseParameters(actualUrl, index);
                        if (_parameters.Count == 0)
                        {
                            _parameters = null;
                        }

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
            return new FtpUrl(url, Scheme, UserName, Password, Host, Port, Path, _parameters, _segments.ToArray());
        }

        private void ParseParameters(StringBuilder actualUrl, int index)
        {
            int lastDelimiter = index;
            index++;
            string currentKey = null;
            string value = null;
            for (; index < actualUrl.Length; index++)
            {
                switch (actualUrl[index])
                {
                    case ';':
                        value = actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1);
                        lastDelimiter = index;
                        if ((String.IsNullOrEmpty(currentKey)) || (String.IsNullOrEmpty(value)))
                        {
                            currentKey = null;
                            break;
                        }

                        _parameters.AddValue(currentKey, value);
                        currentKey = null;
                        break;
                    case '=':
                        currentKey = actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1);
                        lastDelimiter = index;
                        break;
                    case '%':
                        index = DecodeEscape(actualUrl, index);
                        break;
                }
            }

            if ((!String.IsNullOrEmpty(currentKey)) && (!String.IsNullOrEmpty(value = actualUrl.ToString(lastDelimiter + 1, index - lastDelimiter - 1))))
            {
                _parameters.AddValue(currentKey, value);
            }
        }
    }
}