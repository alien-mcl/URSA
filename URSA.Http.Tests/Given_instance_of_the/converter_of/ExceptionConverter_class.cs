using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Converters;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExceptionConverter_class
    {
        private ExceptionConverter _converter;

        [TestMethod]
        public void it_should_provide_problem_description()
        {
            var message = "Test exception";
            var exception = MakeException(message);
            var response = new ExceptionResponseInfo(new RequestInfo(Verb.GET, new Uri("http://temp.uri/"), new MemoryStream(), new BasicClaimBasedIdentity()), exception);

            _converter.ConvertFrom(exception, response);
            response.Body.Seek(0, SeekOrigin.Begin);

            dynamic details = (JObject)new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(response.Body)));

            ((string)details.title).Should().Be(message);
            ((string)details.details).Should().NotBeNull();
            ((int)details.status).Should().Be((int)exception.Status);
        }

        [TestInitialize]
        public void Setup()
        {
            _converter = new ExceptionConverter();
        }

        [TestCleanup]
        public void Teardown()
        {
            _converter = null;
        }

        private ProtocolException MakeException(string message)
        {
            ProtocolException result;
            try
            {
                throw new ProtocolException(HttpStatusCode.BadRequest, message);
            }
            catch (ProtocolException error)
            {
                result = error;
            }

            return result;
        }
    }
}