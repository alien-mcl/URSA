#pragma warning disable 1591
using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using URSA.Web.Http.Description.CodeGen;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class OGuidUriParser_class
    {
        private static readonly Uri Uri = new Uri(OGuidUriParser.OGuid + "guid");
        private OGuidUriParser _parser;

        [Test]
        public void it_should_extract_namespace_from_OpenGuid_uri()
        {
            string @namespace;
            _parser.Parse(Uri, out @namespace);

            @namespace.Should().Be("System");
        }

        [Test]
        public void it_should_extract_name_from_OpenGuid_uri()
        {
            string @namespace;
            var name = _parser.Parse(Uri, out @namespace);

            name.Should().Be("Guid");
        }

        [SetUp]
        public void Setup()
        {
            _parser = new OGuidUriParser();
        }

        [TearDown]
        public void Teardown()
        {
            _parser = null;
        }
    }
}