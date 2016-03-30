using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Parses <see cref="Uri" />s.</summary>
    public class GenericUriParser : IUriParser
    {
        /// <inheritdoc />
        public UriParserCompatibility IsApplicable(Uri uri)
        {
            return UriParserCompatibility.SchemeMatch;
        }

        /// <inheritdoc />
        public string Parse(Uri uri, out string @namespace)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var parts = uri.Segments.Select(item => item.Trim('/').ToUpperCamelCase()).Where(item => item.Length > 0);
            if (uri.Fragment.Length > 1)
            {
                parts = parts.Concat(new[] { uri.Fragment.Substring(1) });
            }

            @namespace = String.Join(".", parts.Take(parts.Count() - 1));
            return parts.Last();
        }
    }
}