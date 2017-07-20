using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using URSA.Configuration;
using URSA.Web.Http;

namespace URSA
{
    /// <summary>Provides an URL parsing facility.</summary>
    public abstract class UrlParser
    {
        /// <summary>Defines an escape indicator.</summary>
        public const char Escape = '%';

        /// <summary>Defines an upper-case alphabet letters.</summary>
        public static readonly char[] HighAlpha = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>Defines a lower-case alphabet letter.</summary>
        public static readonly char[] LowAlpha = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        /// <summary>Defines alphabet letters.</summary>
        public static readonly char[] Alpha = LowAlpha.Concat(HighAlpha).ToArray();

        /// <summary>Defines digits.</summary>
        public static readonly char[] Digit = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>Defines alphabet and digits.</summary>
        public static readonly char[] AlphaDigit = Alpha.Concat(Digit).ToArray();

        /// <summary>Defines safe chars.</summary>
        public static readonly char[] Safe = { '$', '-', '_', '.', '+' };

        /// <summary>Defines additional extra chars.</summary>
        public static readonly char[] Extra = { '!', '*', '\'', '(', ')', ',' };

        /// <summary>Defines unreserved chars.</summary>
        public static readonly char[] Unreserved = Alpha.Concat(Digit).Concat(Safe).Concat(Extra).ToArray();

        /// <summary>Defines unreserved chars with escape char.</summary>
        public static readonly char[] UChar = Unreserved.Concat(new[] { Escape }).ToArray();

        /// <summary>Defines reserved chars.</summary>
        public static readonly char[] Reserved = { ';', '/', '?', ':', '@', '&', '=' };

        /// <summary>Defines any chars.</summary>
        public static readonly char[] XChar = Unreserved.Concat(Reserved).Concat(new[] { Escape }).ToArray();

        /// <summary>Defines chars allowed in scheme.</summary>
        public static readonly char[] SchemeAllowed = LowAlpha.Concat(Digit).Concat(new[] { '+', '-', '.' }).ToArray();

        /// <summary>Defines chars allowed in login part.</summary>
        public static readonly char[] LoginAllowed = UChar.Concat(new[] { ';', '?', '&', '=' }).ToArray();

        /// <summary>Defines chars allowed in host.</summary>
        public static readonly char[] HostAllowed = AlphaDigit.Concat(new[] { '-', '.' }).ToArray();

        private static readonly IDictionary<string, Type> UrlParsers = new ConcurrentDictionary<string, Type>();

        static UrlParser()
        {
            foreach (var entry in UrlParserConfigurationSection.Default.Parsers)
            {
                object[] arguments = { (entry.Value.Any() ? entry.Value.Cast<object>().ToArray() : new string[0]) };
                typeof(UrlParser).GetTypeInfo().DeclaredMethods.First(method => method.Name == "Register").MakeGenericMethod(entry.Key).Invoke(null, arguments);
            }
        }

        /// <summary>Gets the supported schemes.</summary>
        public abstract IEnumerable<string> SupportedSchemes { get; }

        /// <summary>Gets a value indicating whether the parser allows relative addresses.</summary>
        public abstract bool AllowsRelativeAddresses { get; }

        /// <summary>Provides an URL parser for a given <paramref name="scheme" />.</summary>
        /// <param name="scheme">The target scheme of a parser.</param>
        /// <returns>URL parser matching a given <paramref name="scheme" />.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public static UrlParser CreateFor(string scheme)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException("scheme");
            }

            if (scheme.Length == 0)
            {
                throw new ArgumentOutOfRangeException("scheme");
            }

            Type result;
            if (!UrlParsers.TryGetValue(scheme, out result))
            {
                throw new ArgumentOutOfRangeException("scheme", String.Format("No matching parser is available for scheme '{0}'.", scheme));
            }

            return (UrlParser)Activator.CreateInstance(result);
        }

        /// <summary>Registers the specified scheme URL parsers. </summary>
        /// <typeparam name="T">Type of the URL parser.</typeparam>
        /// <param name="schemes">The schemes to register URL parser for.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public static void Register<T>(params string[] schemes) where T : UrlParser, new()
        {
            IEnumerable<string> schemesToRegister = schemes;
            if ((schemes == null) || (schemes.Length == 0))
            {
                var parser = Activator.CreateInstance<T>();
                if ((!(schemesToRegister = parser.SupportedSchemes).Any()) && (parser.AllowsRelativeAddresses))
                {
                    UrlParsers[String.Empty] = typeof(T);
                    return;
                }
            }

            foreach (var scheme in schemesToRegister)
            {
                UrlParsers[scheme] = typeof(T);
            }
        }

        /// <summary>Parses the specified URL.</summary>
        /// <param name="url">The URL string.</param>
        /// <returns>Parsed URL.</returns>
        public static Url Parse(string url)
        {
            string scheme = String.Empty;
            for (int index = 0; index < url.Length; index++)
            {
                char currentChar = url[index];
                switch (currentChar)
                {
                    default:
                        if ((scheme.Length == 0) && (!SchemeAllowed.Contains(Char.ToLowerInvariant(currentChar))))
                        {
                            var parser = (from registeredParser in UrlParsers
                                          let urlParser = (UrlParser)Activator.CreateInstance(registeredParser.Value)
                                          where urlParser.AllowsRelativeAddresses
                                          select urlParser).FirstOrDefault();
                            if (parser == null)
                            {
                                throw new ArgumentOutOfRangeException("url", "Provided url is either malformed or there is no way of parsing it.");
                            }

                            return parser.Parse(url, -1);
                        }

                        break;
                    case ':':
                        if (index == 0)
                        {
                            throw new ArgumentOutOfRangeException("url", "Provided url is malformed as it specify neither scheme nor is a relative one.");
                        }

                        if (scheme.Length == 0)
                        {
                            scheme = url.Substring(0, index).ToLower();
                            var parser = CreateFor(scheme);
                            return parser.Parse(url, index);
                        }

                        break;
                }
            }

            throw new ArgumentOutOfRangeException("url", "Provided url is either malformed or there is no way of parsing it.");
        }

        /// <summary>Escapes a given <paramref name="value" />.</summary>
        /// <param name="value">Value to be escaped.</param>
        /// <param name="allowedChars">Allowed chars.</param>
        /// <returns>Escaped <paramref name="value" /></returns>
        public static string ToSafeString(string value, char[] allowedChars)
        {
            StringBuilder result = new StringBuilder(value);
            for (int index = 0; index < result.Length; index++)
            {
                var currentChar = result[index];
                if (!allowedChars.Contains(currentChar))
                {
                    string replacement = "%" + String.Join("%", Encoding.UTF8.GetBytes(new[] { currentChar }).Select(@byte => @byte.ToString("X")));
                    result.Remove(index, 1).Insert(index, replacement);
                    index += replacement.Length - 1;
                }
            }

            return result.ToString();
        }

        /// <summary>Parses the specified URL.</summary>
        /// <param name="url">The URL to be parsed.</param>
        /// <param name="schemeSpecificPartIndex">Index of the scheme specific part.</param>
        /// <returns>Parsing result.</returns>
        public abstract Url Parse(string url, int schemeSpecificPartIndex);
    }
}