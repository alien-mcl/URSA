using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using URSA;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Converters;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class ExceptionConverter_class
    {
        private ExceptionConverter _converter;

        [Test]
        public void it_should_provide_problem_description()
        {
            var message = "Test exception";
            var exception = MakeException(message);
            var response = new ExceptionResponseInfo(new RequestInfo(Verb.GET, (HttpUrl)UrlParser.Parse("http://temp.uri/"), new MemoryStream(), new BasicClaimBasedIdentity()), exception);

            _converter.ConvertFrom(exception, response);
            response.Body.Seek(0, SeekOrigin.Begin);

            dynamic details = (JObject)new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(response.Body)));

            ((string)details.title).Should().Be(message);
            ((string)details.details).Should().NotBeNull();
            ((int)details.status).Should().Be((int)exception.Status);
        }

        [Test]
        public void it_should_throw_when_no_response_is_provided_for_serialization_compatibility_test()
        {
            _converter.Invoking(instance => instance.CanConvertFrom<Exception>(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("response");
        }

        [Test]
        public void it_should_throw_when_no_given_type_is_provided_for_serialization()
        {
            _converter.Invoking(instance => instance.ConvertFrom(null, null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("givenType");
        }

        [Test]
        public void it_should_throw_when_the_instance_being_serialized_mismatches_a_given_type()
        {
            _converter.Invoking(instance => instance.ConvertFrom(typeof(Exception), DateTime.Now, null)).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void it_should_do_nothing_if_the_instance_being_serialized_is_null()
        {
            _converter.Invoking(instance => instance.ConvertFrom(typeof(Exception), null, null)).ShouldNotThrow();
        }

        [SetUp]
        public void Setup()
        {
            _converter = new ExceptionConverter();
        }

        [TearDown]
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