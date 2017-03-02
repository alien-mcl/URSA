#pragma warning disable 1591
using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using GenericUriParser = URSA.Web.Http.Description.CodeGen.GenericUriParser;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class GenericUriParser_class
    {
        private static readonly Uri Uri = new Uri("http://temp.uri/some/namespace/class");
        private GenericUriParser _parser;

        [Test]
        public void it_should_extract_namespace_from_HTTP_uri()
        {
            string @namespace;
            _parser.Parse(Uri, out @namespace);

            @namespace.Should().Be("Some.Namespace");
        }

        [Test]
        public void it_should_extract_name_from_HTTP_uri()
        {
            string @namespace;
            var name = _parser.Parse(Uri, out @namespace);

            name.Should().Be("Class");
        }

        [SetUp]
        public void Setup()
        {
            _parser = new GenericUriParser();
        }

        [TearDown]
        public void Teardown()
        {
            _parser = null;
        }
    }
}