using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Provides an IP based URL parsing facility.</summary>
    public abstract class IpUrlParser : UrlParser
    {
        private string _scheme = String.Empty;
        private string _userName = String.Empty;
        private string _password = String.Empty;
        private string _host = String.Empty;
        private string _path;
        private ushort _port;

        /// <summary>Defines possible expected URL tokens.</summary>
        [Flags]
        protected enum Expected : uint
        {
            /// <summary>Defines no expectation.</summary>
            Nothing = 0x00000000,

            /// <summary>Defines an user name expectation.</summary>
            UserName = 0x00000001,

            /// <summary>Defines a password expectation.</summary>
            Password = 0x00000002,

            /// <summary>Defines a host expectation.</summary>
            Host = 0x00000004,

            /// <summary>Defines a port expectation.</summary>
            Port = 0x00000008,

            /// <summary>Defines a path expectation.</summary>
            Path = 0x00000010
        }

        /// <summary>Gets or sets the scheme.</summary>
        protected string Scheme { get { return _scheme; } set { _scheme = value ?? String.Empty; } }

        /// <summary>Gets or sets the name of the user.</summary>
        protected string UserName { get { return _userName; } set { _userName = value ?? String.Empty; } }

        /// <summary>Gets or sets the password.</summary>
        protected string Password { get { return _password; } set { _password = value ?? String.Empty; } }

        /// <summary>Gets or sets the host.</summary>
        protected string Host { get { return _host; } set { _host = value ?? String.Empty; } }

        /// <summary>Gets or sets the port.</summary>
        protected ushort Port { get { return _port == 0 ? DefaultPort : _port; } set { _port = value == 0 ? DefaultPort : value; } }

        /// <summary>Gets or sets the path.</summary>
        protected string Path { get { return _path ?? SegmentSeparator.ToString(); } set { _path = value ?? SegmentSeparator.ToString(); } }

        /// <summary>Gets the chars allowed in the URL's path.</summary>
        protected abstract char[] PathAllowed { get; }

        /// <summary>Gets a value indicating whether the URL scheme supports login credentials.</summary>
        protected abstract bool SupportsLogin { get; }

        /// <summary>Gets the default port.</summary>
        protected abstract ushort DefaultPort { get; }

        /// <summary>Gets the segments. </summary>
        protected abstract IList<string> Segments { get; }

        /// <summary>Gets the segment separator.</summary>
        protected virtual char SegmentSeparator { get { return '/'; } }

        /// <inheritdoc />
        public override Url Parse(string url, int schemeSpecificPartIndex)
        {
            if ((schemeSpecificPartIndex >= 0) && (url.Length - schemeSpecificPartIndex < 4))
            {
                throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
            }

            int index = (schemeSpecificPartIndex >= 0 ? schemeSpecificPartIndex + 3 : 0);
            Scheme = (schemeSpecificPartIndex > 0 ? url.Substring(0, schemeSpecificPartIndex) : String.Empty);
            Expected expectedToken = (Scheme.Length > 0 ? Expected.UserName | Expected.Host : Expected.Path);
            int lastDelimiter = index;
            var actualUrl = new StringBuilder(url);
            bool hasPathParsed = false;
            for (; index < actualUrl.Length; index++)
            {
                hasPathParsed = ParseInternal(actualUrl, ref index, ref lastDelimiter, ref expectedToken);
            }

            if (!hasPathParsed)
            {
                FinalizeHostOrPort(actualUrl, index, lastDelimiter, ref expectedToken);
            }

            return CreateInstance(url);
        }

        /// <summary>Creates an instance of the parsed URL.</summary>
        /// <param name="url">Url passed for parsing.</param>
        /// <returns>Instance of the <see cref="IpUrl" />.</returns>
        protected abstract IpUrl CreateInstance(string url);

        /// <summary>Decodes an escaped chars.</summary>
        /// <param name="actualUrl">The actual URL.</param>
        /// <param name="index">Current index.</param>
        /// <returns>New value of the <paramref name="index" />.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">url;Passed url is malformed.</exception>
        protected int DecodeEscape(StringBuilder actualUrl, int index)
        {
            var bytes = new List<byte>();
            int lastDelimiter = index;
            while (index < actualUrl.Length)
            {
                if (actualUrl.Length - index < 3)
                {
                    throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
                }

                bytes.Add(Byte.Parse(actualUrl.ToString(index + 1, 2), NumberStyles.HexNumber));
                if (((index + 3 < actualUrl.Length) && (actualUrl[index + 3] != '%')) || (index + 3 >= actualUrl.Length))
                {
                    break;
                }

                index++;
            }

            var encoded = Encoding.UTF8.GetChars(bytes.ToArray());
            actualUrl.Remove(lastDelimiter, 3 * bytes.Count).Insert(lastDelimiter, encoded);
            return index;
        }

        /// <summary>Parses the path.</summary>
        /// <param name="actualUrl">AN Actual URL.</param>
        /// <param name="index">Current index.</param>
        protected abstract void ParsePath(StringBuilder actualUrl, int index);

        /// <summary>Parses the segment with dot-segment normalization.</summary>
        /// <param name="actualUrl">The actual URL.</param>
        /// <param name="index">Current index.</param>
        /// <param name="lastSegment">The last segment index.</param>
        /// <returns>New index to be set.</returns>
        protected int ParseSegment(StringBuilder actualUrl, int index, ref int lastSegment)
        {
            if (index - lastSegment - 1 > 0)
            {
                string segment = actualUrl.ToString(lastSegment + 1, index - lastSegment - 1);
                switch (segment)
                {
                    case ".":
                        actualUrl.Remove(lastSegment + 1, index - lastSegment);
                        index -= 2;
                        segment = null;
                        break;
                    case "..":
                        actualUrl.Remove(lastSegment + 1, index - lastSegment);
                        index -= 3;
                        segment = null;
                        if (Segments.Count > 0)
                        {
                            int segmentLength = Segments[Segments.Count - 1].Length + 1;
                            actualUrl.Remove(lastSegment = index -= segmentLength, segmentLength);
                            Segments.RemoveAt(Segments.Count - 1);
                        }

                        break;
                }

                if (segment != null)
                {
                    Segments.Add(segment);
                    lastSegment = index;
                }
            }
            else
            {
                lastSegment = index;
            }

            return index;
        }

        private bool ParseInternal(StringBuilder actualUrl, ref int index, ref int lastDelimiter, ref Expected expectedToken)
        {
            char currentChar = actualUrl[index];
            switch (currentChar)
            {
                default:
                    if (currentChar != SegmentSeparator)
                    {
                        ProcessNonPathChar(actualUrl, index, expectedToken);
                        return false;
                    }

                    if (Scheme.Length > 0)
                    {
                        FinalizeHostOrPort(actualUrl, index, lastDelimiter, ref expectedToken);
                    }

                    ParsePath(actualUrl, index);
                    index = actualUrl.Length;
                    return true;
                case '%':
                    index = DecodeEscape(actualUrl, index);
                    return false;
                case ':':
                    lastDelimiter = FinalizeUserNameOrHost(actualUrl, index, lastDelimiter, ref expectedToken);
                    return false;
                case '@':
                    lastDelimiter = FinalizeUserNameOrPassword(actualUrl, index, lastDelimiter, ref expectedToken);
                    return false;
            }
        }

        private void ProcessNonPathChar(StringBuilder actualUrl, int index, Expected expectedToken)
        {
            char currentChar = actualUrl[index];
            if ((((expectedToken & Expected.Host) == Expected.Host) && (!HostAllowed.Contains(currentChar))) ||
                (((expectedToken & (Expected.UserName | Expected.Password)) != Expected.Nothing) && (!LoginAllowed.Contains(currentChar))) ||
                (((expectedToken & Expected.Port) == Expected.Port) && (!Char.IsDigit(currentChar))))
            {
                throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
            }
        }

        private int FinalizeUserNameOrPassword(StringBuilder actualUrl, int index, int lastDelimiter, ref Expected expectedToken)
        {
            if (!SupportsLogin)
            {
                throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
            }

            if ((expectedToken & Expected.Password) == Expected.Password)
            {
                Password = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                expectedToken = Expected.Host;
                return index + 1;
            }
            
            if ((expectedToken & Expected.UserName) == Expected.UserName)
            {
                UserName = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                expectedToken = Expected.Host;
                return index + 1;
            }

            throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
        }

        private int FinalizeUserNameOrHost(StringBuilder actualUrl, int index, int lastDelimiter, ref Expected expectedToken)
        {
            if (((expectedToken & Expected.UserName) == Expected.UserName) && (SupportsLogin))
            {
                UserName = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                expectedToken = Expected.Password;
                return index + 1;
            }
            
            if ((expectedToken & Expected.Host) == Expected.Host)
            {
                Host = actualUrl.ToString(lastDelimiter, index - lastDelimiter).ToLowerInvariant();
                expectedToken = Expected.Port;
                return index + 1;
            }
            
            throw new ArgumentOutOfRangeException("url", "Passed url is malfored.");
        }

        private void FinalizeHostOrPort(StringBuilder actualUrl, int index, int lastDelimiter, ref Expected expectedToken)
        {
            if ((expectedToken & Expected.Host) == Expected.Host)
            {
                if ((Host = actualUrl.ToString(lastDelimiter, index - lastDelimiter).ToLowerInvariant()).Length == 0)
                {
                    throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
                }
            }
            else if ((expectedToken & Expected.Port) == Expected.Port)
            {
                string port = actualUrl.ToString(lastDelimiter, index - lastDelimiter);
                if (port.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("url", "Passed url is malfored.");
                }

                Port = UInt16.Parse(port);
            }
            else
            {
                throw new ArgumentOutOfRangeException("url", "Passed url is malformed.");
            }

            expectedToken = Expected.Path;
        }
    }
}