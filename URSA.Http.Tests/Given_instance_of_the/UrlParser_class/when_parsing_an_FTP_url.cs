using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Testing;

namespace Given_instance_of_the.UrlParser_class
{
    [TestClass]
    public class when_parsing_an_FTP_url
    {
        private static readonly IDictionary<string, UrlScenario> ValidScenarios = new Dictionary<string, UrlScenario>()
            {
                { "ftp://t", new UrlScenario() { Scheme = "ftp", Host = "t", Path = "/", AsString = "ftp://t/" } },
                { "ftp://temp.uri/", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/", AsString = "ftp://temp.uri/" } },
                { "ftp://temp.uri/test", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/test", AsString = "ftp://temp.uri/test" } },
                { "ftp://temp.uri/test%3Fcom", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/test?com", AsString = "ftp://temp.uri/test%3Fcom" } },
                { "ftp://temp.uri/test/whatever", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/test/whatever", AsString = "ftp://temp.uri/test/whatever" } },
                { "ftp://userName@temp.uri/test/whatever", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", UserName = "userName", Path = "/test/whatever", AsString = "ftp://userName@temp.uri/test/whatever" } },
                { "ftp://userName@temp.uri:22/test/whatever", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", UserName = "userName", Port = 22, Path = "/test/whatever", AsString = "ftp://userName@temp.uri:22/test/whatever" } },
                { "ftp://userName:P%40$$w0rd@temp.uri/test/whatever", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", UserName = "userName", Password = "P@$$w0rd", Path = "/test/whatever", AsString = "ftp://userName:P%40$$w0rd@temp.uri/test/whatever" } },
                { "ftp://userName:P%40$$w0rd@temp.uri:22/test/whatever", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", UserName = "userName", Password = "P@$$w0rd", Port = 22, Path = "/test/whatever", AsString = "ftp://userName:P%40$$w0rd@temp.uri:22/test/whatever" } },
                { "ftp://temp.uri/test/whatever;", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/test/whatever", AsString = "ftp://temp.uri/test/whatever" } },
                { "ftp://temp.uri/test/whatever;type", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/test/whatever", AsString = "ftp://temp.uri/test/whatever" } },
                { "ftp://temp.uri/test/whatever;type=i", new UrlScenario() { Scheme = "ftp", Host = "temp.uri", Path = "/test/whatever", Parameters = new[] { new KeyValuePair<string, string>("type", "i") }, AsString = "ftp://temp.uri/test/whatever;type=i" } },
            };

        private static readonly string[] InvalidScenarios = 
            {
                "ftp://",
                "ftp://userName:/",
                "ftp://userName:P%4",
                "ftp://userName:P%40$$w0rd/",
                "ftp://userName:P%40$$w0rd@/"
            };

        [TestMethod]
        public void it_should_parse_Url()
        {
            foreach (var scenario in ValidScenarios)
            {
                var result = (FtpUrl)new FtpUrlParser().Parse(scenario.Key, 3);
                result.Should().ComplyWith(scenario.Value);
            }
        }

        [TestMethod]
        public void it_should_throw_on_invalid_Url()
        {
            foreach (var scenario in InvalidScenarios)
            {
                new FtpUrlParser().Invoking(instance => instance.Parse(scenario, 3)).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("url");
            }
        }
    }
}