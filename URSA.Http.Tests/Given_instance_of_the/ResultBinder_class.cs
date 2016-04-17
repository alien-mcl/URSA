using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using URSA.Security;
using URSA.Web.Converters;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Tests.Data;

namespace Given_instance_of_the
{
    [TestClass]
    public class ResultBinder_class
    {
        private Mock<IConverterProvider> _converterProvider;
        private Mock<IConverter> _converter;
        private IResultBinder _resultBinder;

        [TestMethod]
        public void it_should_bind_response_object()
        {
            var expected = new Person() { Key = 1, FirstName = "test", LastName = "test", Roles = new[] { "test" } };
            var body = new MemoryStream();
            new JsonSerializer().Serialize(new StreamWriter(body), expected);
            body.Seek(0, SeekOrigin.Begin);
            var request = new RequestInfo(Verb.GET, new Uri("http://temp.uri/"), body, new BasicClaimBasedIdentity(), new Header("Content-Type", "application/json"));
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Person), request, false)).Returns(_converter.Object);
            _converter.Setup(instance => instance.ConvertTo(typeof(Person), request)).Returns(expected);

            var result = _resultBinder.BindResults<Person>(request);

            result.Should().HaveCount(1);
            result[0].Should().Be(expected);
        }

        [TestMethod]
        public void it_should_bind_response_from_header()
        {
            var expected = Guid.NewGuid();
            var request = new RequestInfo(
                Verb.GET,
                new Uri("http://temp.uri/"),
                new MemoryStream(),
                new BasicClaimBasedIdentity(),
                new Header("Content-Type", "text/plain"),
                new Header("Location", "http://temp.uri/" + expected));
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Guid), request, false)).Returns((IConverter)null);

            var result = _resultBinder.BindResults<Guid>(request);

            result.Should().HaveCount(1);
            result[0].Should().Be(expected);
        }

        [TestMethod]
        public void it_should_throw_when_no_converter_provider_is_passed()
        {
            ((ResultBinder)null).Invoking(_ => new ResultBinder(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("converterProvider");
        }

        [TestMethod]
        public void it_should_throw_when_no_primary_result_type_is_given()
        {
            ((ResultBinder)_resultBinder).Invoking(instance => instance.BindResults(null, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("primaryResultType");
        }

        [TestMethod]
        public void it_should_throw_when_no_request_is_given()
        {
            ((ResultBinder)_resultBinder).Invoking(instance => instance.BindResults(typeof(int), null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("requestInfo");
        }

        [TestInitialize]
        public void Setup()
        {
            _converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            _converter = new Mock<IConverter>(MockBehavior.Strict);
            _resultBinder = new ResultBinder(_converterProvider.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _converter = null;
            _converterProvider = null;
            _resultBinder = null;
        }
    }
}
