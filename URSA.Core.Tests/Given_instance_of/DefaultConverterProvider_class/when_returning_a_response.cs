using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using URSA.Web;
using URSA.Web.Converters;

namespace Given_instance_of.DefaultConverterProvider_class
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class when_returning_a_response
    {
        private IList<IConverter> _converters;
        private Mock<IResponseInfo> _response;
        private Mock<IRequestInfo> _request;
        private Mock<IConverter> _stringConverter;
        private Mock<IConverter> _uriConverter;
        private IConverterProvider _provider;

        [TestMethod]
        public void it_should_provide_string_converter()
        {
            var match = _provider.FindBestOutputConverter<string>(_response.Object);

            match.Should().Be(_stringConverter.Object);
            _stringConverter.Verify(instance => instance.CanConvertFrom(typeof(string), _response.Object), Times.Once);
            _uriConverter.Verify(instance => instance.CanConvertFrom(typeof(string), _response.Object), Times.Once);
        }

        [TestMethod]
        public void it_should_provide_Uri_converter()
        {
            var match = _provider.FindBestOutputConverter(typeof(Uri), _response.Object);

            match.Should().Be(_uriConverter.Object);
            _stringConverter.Verify(instance => instance.CanConvertFrom(typeof(Uri), _response.Object), Times.Once);
            _uriConverter.Verify(instance => instance.CanConvertFrom(typeof(Uri), _response.Object), Times.Once);
        }

        [TestMethod]
        public void it_should_throw_when_not_initialized_and_trying_to_find_output_converter()
        {
            var provider = new DefaultConverterProvider();
            provider.Invoking(instance => instance.FindBestOutputConverter(typeof(string), new Mock<IResponseInfo>().Object)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void it_should_throw_when_looking_for_output_converter_without_specifying_target_type()
        {
            _provider.Invoking(instance => instance.FindBestOutputConverter(null, new Mock<IResponseInfo>().Object)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void it_should_throw_when_looking_for_output_converter_without_response_details()
        {
            _provider.Invoking(instance => instance.FindBestOutputConverter(typeof(string), null)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void it_should_provide_any_converter_when_request_expects_any_media_type()
        {
            _request.SetupGet(instance => instance.OutputNeutral).Returns(true);
            _stringConverter.Setup(instance => instance.CanConvertTo(typeof(int), _request.Object)).Returns(CompatibilityLevel.None);
            _uriConverter.Setup(instance => instance.CanConvertTo(typeof(int), _request.Object)).Returns(CompatibilityLevel.None);
            var intConverter = new Mock<IConverter>(MockBehavior.Strict);
            intConverter.Setup(instance => instance.CanConvertFrom(It.IsAny<Type>(), _response.Object)).Returns(CompatibilityLevel.None);
            intConverter.Setup(instance => instance.CanConvertTo(typeof(int), _request.Object)).Returns(CompatibilityLevel.TypeMatch);
            _converters.Add(intConverter.Object);
            
            var result = _provider.FindBestOutputConverter(typeof(int), _response.Object);

            result.Should().Be(intConverter.Object);
        }

        [TestMethod]
        public void it_should_not_provide_any_converter_when_not_matching_the_request_are_found()
        {
            _request.SetupGet(instance => instance.OutputNeutral).Returns(false);
            _stringConverter.Setup(instance => instance.CanConvertTo(typeof(int), _request.Object)).Returns(CompatibilityLevel.None);
            _uriConverter.Setup(instance => instance.CanConvertTo(typeof(int), _request.Object)).Returns(CompatibilityLevel.None);

            var result = _provider.FindBestOutputConverter(typeof(int), _response.Object);

            result.Should().BeNull();
        }

        [TestInitialize]
        public void Setup()
        {
            _request = new Mock<IRequestInfo>(MockBehavior.Strict);
            _response = new Mock<IResponseInfo>(MockBehavior.Strict);
            _response.SetupGet(instance => instance.Request).Returns(_request.Object);
            _stringConverter = new Mock<IConverter>(MockBehavior.Strict);
            _stringConverter.Setup(instance => instance.CanConvertFrom(It.IsAny<Type>(), _response.Object))
                .Returns<Type, IResponseInfo>((type, response) => type == typeof(string) ? CompatibilityLevel.ExactMatch : CompatibilityLevel.None);
            _uriConverter = new Mock<IConverter>(MockBehavior.Strict);
            _uriConverter.Setup(instance => instance.CanConvertFrom(It.IsAny<Type>(), _response.Object))
                .Returns<Type, IResponseInfo>((type, response) => type == typeof(Uri) ? CompatibilityLevel.ExactMatch : CompatibilityLevel.None);
            _provider = new DefaultConverterProvider();
            _converters = new List<IConverter>() { _stringConverter.Object, _uriConverter.Object };
            _provider.Initialize(() => _converters);
        }

        [TestCleanup]
        public void Teardown()
        {
            _request = null;
            _stringConverter = null;
            _uriConverter = null;
            _provider = null;
        }
    }
}