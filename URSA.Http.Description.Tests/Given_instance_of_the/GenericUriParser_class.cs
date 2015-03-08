using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericUriParser = URSA.Web.Http.Description.CodeGen.GenericUriParser;

namespace Given_instance_of_the
{
    [TestClass]
    public class GenericUriParser_class
    {
        private static readonly Uri Uri = new Uri("http://temp.uri/some/namespace/class");
        private GenericUriParser _parser;

        [TestMethod]
        public void it_should_extract_namespace_from_HTTP_uri()
        {
            string @namespace;
            _parser.Parse(Uri, out @namespace);

            @namespace.Should().Be("Some.Namespace");
        }

        [TestMethod]
        public void it_should_extract_name_from_HTTP_uri()
        {
            string @namespace;
            var name = _parser.Parse(Uri, out @namespace);

            name.Should().Be("Class");
        }

        [TestInitialize]
        public void Setup()
        {
            _parser = new GenericUriParser();
        }

        [TestCleanup]
        public void Teardown()
        {
            _parser = null;
        }
    }
}