using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace URSA.Core.Tests.Given_instance_of
{
    [TestClass]
    public class Url_class
    {
        private const string Scheme = "test";
        private const string Path = "location";

        [TestMethod]
        public void it_should_convert_null_Url_to_null_Uri()
        {
            Url url = null;
            ((Uri)url).Should().BeNull();
        }

        [TestMethod]
        public void it_should_convert_null_Uri_to_null_Url()
        {
            Uri uri = null;
            ((Url)uri).Should().BeNull();
        }

        [TestMethod]
        public void it_should_convert_Url_to_Uri()
        {
            var url = new FakeUrl();

            var result = (Uri)url;

            result.Should().BeOfType<Uri>().Which.AbsoluteUri.Should().Be(Scheme + ":" + Path);
        }

        [TestMethod]
        public void it_should_convert_Uri_to_Url()
        {
            var uri = new Uri(Scheme + ":" + Path);
            UrlParser.Register<FakeUrlParser>("test");

            var result = (Url)uri;

            result.Should().BeOfType<FakeUrl>();
        }

        private class FakeUrlParser : UrlParser
        {
            public override IEnumerable<string> SupportedSchemes { get { return new[] { Scheme }; } }

            public override bool AllowsRelativeAddresses { get { return false; } }

            public override Url Parse(string url, int schemeSpecificPartIndex)
            {
                return new FakeUrl();
            }
        }

        private class FakeUrl : Url
        {
            public override string Scheme { get { return Url_class.Scheme; } }

            public override string Location { get { return Path; } }

            public override string Host { get { return "host"; } }

            public override ushort Port { get { return 1; } }

            public override string OriginalUrl { get { return Scheme + ":" + Path; } }
        }
    }
}
