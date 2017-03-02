using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [TestFixture]
    public class WebRequestProvider_class
    {
        private IWebRequestProvider _webRequestProvider;

        [Test]
        public void it_should_create_a_web_request()
        {
            var expected = "http://temp.uri/";
            var result = _webRequestProvider.CreateRequest(new Uri(expected), new Dictionary<string, string>() { { "Accept", "*/*" }, { "Location", expected } });

            result.Should().BeOfType<HttpWebRequest>().Which.Headers["Location"].Should().Be(expected);
        }

        [Test]
        public void it_should_throw_when_no_uri_is_provided()
        {
            _webRequestProvider.Invoking(instance => instance.CreateRequest(null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("uri");
        }

        [Test]
        public void it_should_throw_when_the_uris_scheme_is_not_supported()
        {
            _webRequestProvider.Invoking(instance => instance.CreateRequest(new Uri("ftp://temp.uri/"), null)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("uri");
        }

        [SetUp]
        public void Setup()
        {
            _webRequestProvider = new WebRequestProvider();
        }

        [TearDown]
        public void Teardown()
        {
            _webRequestProvider = null;
        }
    }
}
