using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using URSA.Web;
using URSA.Web.Converters;

namespace Given_instance_of
{
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
            var match = _provider.FindBestInputConverter(typeof(string), _request.Object);

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
    }
}