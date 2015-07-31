using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Parses URN based <see cref="Uri" />s.</summary>
    public class HydraUriParser : IUriParser
    {
        /// <inheritdoc />
        public UriParserCompatibility IsApplicable(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Scheme != "urn")
            {
                return UriParserCompatibility.None;
            }

            string segment = null;
            if ((uri.Segments.Length > 0) && ((segment = uri.Segments.First()) != null) &&
                ((segment.StartsWith(Reflection.TypeExtensions.DotNetSymbol)) ||
                 (segment.StartsWith(Reflection.TypeExtensions.DotNetListSymbol)) ||
                 (segment.StartsWith(Reflection.TypeExtensions.HydraSymbol))))
            {
                return UriParserCompatibility.ExactMatch;
            }

            return UriParserCompatibility.SchemeMatch;
        }

        /// <inheritdoc />
        public string Parse(Uri uri, out string @namespace)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            string language = null;
            var name = uri.Segments.First();
            var colonIndex = -1;
            if ((colonIndex = name.IndexOf(':')) != -1)
            {
                language = name.Substring(0, colonIndex);
                name = name.Substring(colonIndex + 1);
            }

            switch (language)
            {
                case Reflection.TypeExtensions.DotNetListSymbol:
                case Reflection.TypeExtensions.DotNetEnumerableSymbol:
                case Reflection.TypeExtensions.DotNetSymbol:
                case Reflection.TypeExtensions.HydraSymbol:
                    break;
                default:
                    name = Regex.Replace(Regex.Replace(name, "[\\/]", "."), "^a-zA-Z0-9_]", String.Empty);
                    break;
            }

            var parts = name.Split('.');
            @namespace = String.Join(".", parts.Take(parts.Length - 1).Select(item => item.ToUpperCamelCase()));
            return parts[parts.Length - 1].ToUpperCamelCase();
        }
    }
}