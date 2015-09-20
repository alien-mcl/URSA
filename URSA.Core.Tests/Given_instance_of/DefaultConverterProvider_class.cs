using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web;
using URSA.Web.Converters;

namespace Given_instance_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DefaultConverterProvider_class
    {
        private Mock<IRequestInfo> _request;
        private Mock<IConverter> _stringConverter;
        private Mock<IConverter> _uriConverter;
        private IConverterProvider _provider;

        [TestMethod]
        public void it_should_provide_string_converter()
        {
            var match = _provider.FindBestInputConverter<string>(_request.Object);

            match.Should().Be(_stringConverter.Object);
            _stringConverter.Verify(instance => instance.CanConvertTo(typeof(string), _request.Object), Times.Once);
            _uriConverter.Verify(instance => instance.CanConvertTo(typeof(string), _request.Object), Times.Once);
        }

        [TestMethod]
        public void it_should_provide_Uri_converter()
        {
            var match = _provider.FindBestInputConverter(typeof(Uri), _request.Object);

            match.Should().Be(_uriConverter.Object);
            _stringConverter.Verify(instance => instance.CanConvertTo(typeof(Uri), _request.Object), Times.Once);
            _uriConverter.Verify(instance => instance.CanConvertTo(typeof(Uri), _request.Object), Times.Once);
        }

        [TestMethod]
        public void it_should_throw_when_initializing_without_converters()
        {
            ArgumentNullException exception = null;
            try
            {
                new DefaultConverterProvider().Initialize(null);
            }
            catch (ArgumentNullException error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
        }

        [TestMethod]
        public void it_should_throw_when_not_initialized_and_trying_to_find_input_conveter()
        {
            InvalidOperationException exception = null;
            try
            {
                new DefaultConverterProvider().FindBestInputConverter(typeof(string), _request.Object);
            }
            catch (InvalidOperationException error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
        }

        [TestMethod]
        public void it_should_throw_when_looking_for_input_converter_without_specifying_target_type()
        {
            _provider.Invoking(instance => instance.FindBestInputConverter(null, _request.Object)).ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void it_should_throw_when_looking_for_input_converter_without_request_details()
        {
            _provider.Invoking(instance => instance.FindBestInputConverter(typeof(string), null)).ShouldThrow<ArgumentNullException>();
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

        [TestInitialize]
        public void Setup()
        {
            _request = new Mock<IRequestInfo>(MockBehavior.Strict);
            _stringConverter = new Mock<IConverter>(MockBehavior.Strict);
            _stringConverter.Setup(instance => instance.CanConvertTo(It.IsAny<Type>(), _request.Object))
                .Returns<Type, IRequestInfo>(
                    (type, request) => type == typeof(string) ? CompatibilityLevel.ExactMatch : CompatibilityLevel.None);
            _uriConverter = new Mock<IConverter>(MockBehavior.Strict);
            _uriConverter.Setup(instance => instance.CanConvertTo(It.IsAny<Type>(), _request.Object))
                .Returns<Type, IRequestInfo>(
                (type, request) => type == typeof(Uri) ? CompatibilityLevel.ExactMatch : CompatibilityLevel.None);
            _provider = new DefaultConverterProvider();
            _provider.Initialize(new[] { _stringConverter.Object, _uriConverter.Object });
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