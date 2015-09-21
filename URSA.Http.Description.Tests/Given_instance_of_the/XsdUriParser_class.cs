#pragma warning disable 1591
using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http.Description.CodeGen;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XsdUriParser_class
    {
        private static readonly Uri Uri = new Uri(XsdUriParser.Xsd + "string");
        private XsdUriParser _parser;

        [TestMethod]
        public void it_should_extract_namespace_from_XSD_uri()
        {
            string @namespace;
            _parser.Parse(Uri, out @namespace);

            @namespace.Should().Be("System");
        }

        [TestMethod]
        public void it_should_extract_name_from_XSD_uri()
        {
            string @namespace;
            var name = _parser.Parse(Uri, out @namespace);

            name.Should().Be("String");
        }

        [TestInitialize]
        public void Setup()
        {
            _parser = new XsdUriParser();
        }

        [TestCleanup]
        public void Teardown()
        {
            _parser = null;
        }
    }
}