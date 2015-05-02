using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UriConverter_class : ConverterTest<UriConverter, Uri>
    {
        private const string ContentType = "text/uri-list";
        private static readonly Uri Entity = new Uri("http://temp.org/");
        private static readonly Uri[] Entities = { new Uri("http://temp.org/"), new Uri("ftp://test.com/test#") };

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        protected override Uri SingleEntity { get { return Entity; } }

        protected override Uri[] MultipleEntities { get { return Entities; } }
    }
}