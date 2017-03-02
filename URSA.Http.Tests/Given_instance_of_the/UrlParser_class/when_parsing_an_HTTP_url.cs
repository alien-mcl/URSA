using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Testing;

namespace Given_instance_of_the.UrlParser_class
{
    [TestFixture]
    public class when_parsing_an_HTTP_url
    {
        private static readonly IDictionary<string, UrlScenario> ValidScenarios = new Dictionary<string, UrlScenario>()
            {
                { "http://t", new UrlScenario() { Scheme = "http", Host = "t", Path = "/", AsString = "http://t/" } },
                { "http://temp.uri", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/", AsString = "http://temp.uri/" } },
                { "http://temp.uri:88", new UrlScenario() { Scheme = "http", Host = "temp.uri", Port = 88, Path = "/", AsString = "http://temp.uri:88/" } },
                { "http://temp.uri:80", new UrlScenario() { Scheme = "http", Host = "temp.uri", Port = 80, Path = "/", AsString = "http://temp.uri/" } },
                { "http://temp.uri/", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/", AsString = "http://temp.uri/" } },
                { "http://temp.uri:88/", new UrlScenario() { Scheme = "http", Host = "temp.uri", Port = 88, Path = "/", AsString = "http://temp.uri:88/" } },
                { "http://temp.uri/test", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test", AsString = "http://temp.uri/test" } },
                { "http://temp.uri:88/test", new UrlScenario() { Scheme = "http", Host = "temp.uri", Port = 88, Path = "/test", AsString = "http://temp.uri:88/test" } },
                { "http://temp.uri/test%3Fcom", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test?com", AsString = "http://temp.uri/test%3Fcom" } },
                { "http://temp.uri/../", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/", AsString = "http://temp.uri/" } },
                { "http://temp.uri/./test/../path", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/path", AsString = "http://temp.uri/path" } },
                { "http://temp.uri/././../test/./../path/is/obviously/./../../../stupid", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/stupid", AsString = "http://temp.uri/stupid" } },
                { "http://temp.uri/test/whatever", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", AsString = "http://temp.uri/test/whatever" } },
                { "http://temp.uri/test/whatever?", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new KeyValuePair<string, string>[0], AsString = "http://temp.uri/test/whatever?" } },
                { "http://temp.uri/test/whatever?query", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("query", String.Empty) }, AsString = "http://temp.uri/test/whatever?query" } },
                { "http://temp.uri/test/whatever?key=value", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("key", "value") }, AsString = "http://temp.uri/test/whatever?key=value" } },
                { "http://temp.uri/test/whatever/.?key=value", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("key", "value") }, AsString = "http://temp.uri/test/whatever?key=value" } },
                { "http://temp.uri/test/whatever/..?key=value", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test", Parameters = new[] { new KeyValuePair<string, string>("key", "value") }, AsString = "http://temp.uri/test?key=value" } },
                { "http://temp.uri:88/test/whatever?key=value", new UrlScenario() { Scheme = "http", Host = "temp.uri", Port = 88, Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("key", "value") }, AsString = "http://temp.uri:88/test/whatever?key=value" } },
                { "http://temp.uri/test/whatever?key=value1&key=value2", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("key", "value1"), new KeyValuePair<string, string>("key", "value2") }, AsString = "http://temp.uri/test/whatever?key=value1&key=value2" } },
                { "http://temp.uri/test/whatever?query#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("query", String.Empty) }, Fragment = "fragment", AsString = "http://temp.uri/test/whatever?query#fragment" } },
                { "http://temp.uri/test/whatever#", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Fragment = String.Empty, AsString = "http://temp.uri/test/whatever#" } },
                { "http://temp.uri/test/whatever#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Fragment = "fragment", AsString = "http://temp.uri/test/whatever#fragment" } },
                { "http://temp.uri/test/whatever/.#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Fragment = "fragment", AsString = "http://temp.uri/test/whatever#fragment" } },
                { "http://temp.uri/test/whatever/..#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test", Fragment = "fragment", AsString = "http://temp.uri/test#fragment" } },
                { "http://temp.uri/test/whatever#fragment?http://test.com/path?query#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Fragment = "fragment?http://test.com/path?query#fragment", AsString = "http://temp.uri/test/whatever#fragment%3Fhttp:%2F%2Ftest.com%2Fpath%3Fquery%23fragment" } },
                { "http://temp.uri/test/whatever#fragment?http://test.com:88/path?query#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Fragment = "fragment?http://test.com:88/path?query#fragment", AsString = "http://temp.uri/test/whatever#fragment%3Fhttp:%2F%2Ftest.com:88%2Fpath%3Fquery%23fragment" } },
                { "http://temp.uri/test/whatever?url=http://test.com/path?query#fragment?http://test.com/path?query#fragment", new UrlScenario() { Scheme = "http", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("url", "http://test.com/path?query") }, Fragment = "fragment?http://test.com/path?query#fragment", AsString = "http://temp.uri/test/whatever?url=http:%2F%2Ftest.com%2Fpath%3Fquery#fragment%3Fhttp:%2F%2Ftest.com%2Fpath%3Fquery%23fragment" } }
            };

        private static readonly string[] InvalidScenarios = 
            {
                "http://",
                "http://userName:/",
                "http://userName:P%4",
                "http://userName:P%40$$w0rd/",
                "http://userName:P%40$$w0rd@/",
                "http://userName@temp.uri/",
                "http://userName:P%40$$w0rd@temp.uri/",
                "http://temp.uri:/",
            };

        [Test]
        public void it_should_parse_Url()
        {
            foreach (var scenario in ValidScenarios)
            {
                var result = (HttpUrl)new HttpUrlParser().Parse(scenario.Key, 4);
                result.Should().ComplyWith(scenario.Value);
            }
        }

        [Test]
        public void it_should_throw_on_invalid_Url()
        {
            foreach (var scenario in InvalidScenarios)
            {
                new HttpUrlParser().Invoking(instance => instance.Parse(scenario, 4)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
            }
        }
    }
}