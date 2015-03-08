using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Parses XSD based <see cref="Uri" />s.</summary>
    public class XsdUriParser : SpecializedUriParser
    {
        internal const string Xsd = "http://www.w3.org/2001/XMLSchema#";

        internal static readonly IDictionary<Type, Uri> Types = new Dictionary<Type, Uri>
        {
            { typeof(byte[]), new Uri(XsdUriParser.Xsd + "hexBinary") },
            { typeof(string), new Uri(XsdUriParser.Xsd + "string") },
            { typeof(bool), new Uri(XsdUriParser.Xsd + "boolean") },
            { typeof(sbyte), new Uri(XsdUriParser.Xsd + "byte") },
            { typeof(byte), new Uri(XsdUriParser.Xsd + "unsignedByte") },
            { typeof(short), new Uri(XsdUriParser.Xsd + "short") },
            { typeof(ushort), new Uri(XsdUriParser.Xsd + "unsignedShort") },
            { typeof(int), new Uri(XsdUriParser.Xsd + "int") },
            { typeof(uint), new Uri(XsdUriParser.Xsd + "unsignedInt") },
            { typeof(long), new Uri(XsdUriParser.Xsd + "long") },
            { typeof(ulong), new Uri(XsdUriParser.Xsd + "unsignedLong") },
            { typeof(float), new Uri(XsdUriParser.Xsd + "float") },
            { typeof(double), new Uri(XsdUriParser.Xsd + "double") },
            { typeof(decimal), new Uri(XsdUriParser.Xsd + "decimal") },
            { typeof(DateTime), new Uri(XsdUriParser.Xsd + "dateTime") },
            { typeof(TimeSpan), new Uri(XsdUriParser.Xsd + "duration") },
            { typeof(Uri), new Uri(XsdUriParser.Xsd + "anyUri") }
        };

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<Type, Uri>> TypeDescriptions { get { return Types; } }
    }
}