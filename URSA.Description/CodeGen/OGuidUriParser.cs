using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Parses <![CDATA[Open Guid]]> based <see cref="Uri" />s.</summary>
    public class OGuidUriParser : SpecializedUriParser
    {
        internal const string OGuid = "http://openguid.net/rdf#";

        internal static readonly IDictionary<Type, Uri> Types = new Dictionary<Type, Uri>
        {
            { typeof(Guid), new Uri(OGuidUriParser.OGuid + "guid") },
            { typeof(Guid).MakeByRefType(), new Uri(OGuidUriParser.OGuid + "guid") }
        };

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<Type, Uri>> TypeDescriptions { get { return Types; } }
    }
}