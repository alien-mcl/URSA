using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using URSA;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Description;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestFixture]
    public class OptionsController_class
    {
        [Test]
        public void it_should_create_an_instance_correctly()
        {
            var result = OptionsController.CreateOperationInfo(typeof(TestController).GetTypeInfo().GetMethod("Add").ToOperationInfo("/", Verb.GET));

            result.ProtocolSpecificCommand.Should().Be(Verb.OPTIONS);
        }

        [Test]
        public void it_should_allow_methods()
        {
            var controller = new OptionsController(HttpStatusCode.MethodNotAllowed, "GET");
            controller.Response = new StringResponseInfo(String.Empty, new RequestInfo(Verb.POST, (HttpUrl)UrlParser.Parse("/"), new MemoryStream(), new BasicClaimBasedIdentity()));

            controller.Allow();

            controller.Response.Headers["Allow"].Should().Be("GET");
        }

        [Test]
        public void it_should_allow_methods_in_CORS_request()
        {
            var controller = new OptionsController(HttpStatusCode.MethodNotAllowed, "GET");
            var request = new RequestInfo(Verb.OPTIONS, (HttpUrl)UrlParser.Parse("/"), new MemoryStream(), new BasicClaimBasedIdentity(), new Header("Origin", "temp.uri"), new Header("Access-Control-Request-Method", "GET"));
            controller.Response = new StringResponseInfo(String.Empty, request);

            controller.Allow();

            controller.Response.Headers["Access-Control-Allow-Methods"].Should().Be("GET");
        }
    }
}