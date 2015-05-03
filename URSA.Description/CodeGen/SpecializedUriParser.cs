using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Provides an abstract infrastructure for uri-specific parsers.</summary>
    public abstract class SpecializedUriParser : IUriParser
    {
        /// <summary>Gets the type descriptions.</summary>
        protected abstract IEnumerable<KeyValuePair<Type, Uri>> TypeDescriptions { get; }

        /// <inheritdoc />
        public UriParserCompatibility IsApplicable(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            return (TypeDescriptions.Any(item => item.Value.ToString() == uri.ToString()) ? UriParserCompatibility.ExactMatch : UriParserCompatibility.None);
        }

        /// <inheritdoc />
        public string Parse(Uri uri, out string @namespace)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            string ns = null;
            var result = (from type in TypeDescriptions
                          where (type.Value.ToString() == uri.ToString()) && (!String.IsNullOrEmpty(ns = type.Key.Namespace))
                          select type.Key.Name).First();
            @namespace = ns;
            return result;
        }
    }
}