﻿using System;
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
            { typeof(byte[]), new Uri(Xsd + "hexBinary") },
            { typeof(string), new Uri(Xsd + "string") },
            { typeof(bool), new Uri(Xsd + "boolean") },
            { typeof(sbyte), new Uri(Xsd + "byte") },
            { typeof(byte), new Uri(Xsd + "unsignedByte") },
            { typeof(short), new Uri(Xsd + "short") },
            { typeof(ushort), new Uri(Xsd + "unsignedShort") },
            { typeof(int), new Uri(Xsd + "int") },
            { typeof(uint), new Uri(Xsd + "unsignedInt") },
            { typeof(long), new Uri(Xsd + "long") },
            { typeof(ulong), new Uri(Xsd + "unsignedLong") },
            { typeof(float), new Uri(Xsd + "float") },
            { typeof(double), new Uri(Xsd + "double") },
            { typeof(decimal), new Uri(Xsd + "decimal") },
            { typeof(DateTime), new Uri(Xsd + "dateTime") },
            { typeof(TimeSpan), new Uri(Xsd + "duration") },
            { typeof(Uri), new Uri(Xsd + "anyUri") }
        };

        /// <inheritdoc />
        protected override IEnumerable<KeyValuePair<Type, Uri>> TypeDescriptions { get { return Types; } }
    }
}