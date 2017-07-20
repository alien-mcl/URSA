using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Tests.Data;

namespace Given_instance_of_the.converter_of
{
    [TestFixture]
    public class Converter_extension
    {
        private Mock<IConverterProvider> _converterProvider;

        [Test]
        public void it_should_return_null_when_no_value_to_be_deserialized_is_provided()
        {
            _converterProvider.Object.ConvertTo(null, typeof(string)).Should().BeNull();
        }

        [Test]
        public void it_should_throw_when_no_type_while_deserializing_is_provided()
        {
            _converterProvider.Object.Invoking(instance => instance.ConvertTo("test", null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("type");
        }

        [Test]
        public void it_should_deserialize_using_type_converters()
        {
            _converterProvider.Object.ConvertTo("1", typeof(int)).Should().Be(1);
        }

        [Test]
        public void it_should_throw_when_no_converter_provider_is_passed_when_deserializing()
        {
            ((IConverterProvider)null).Invoking(instance => instance.ConvertTo(String.Empty, typeof(Exception), new Mock<IRequestInfo>(MockBehavior.Strict).Object))
                .ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("converterProvider");
        }

        [Test]
        public void it_should_deserialize_using_custom_converters()
        {
            var request = new Mock<IRequestInfo>(MockBehavior.Strict);
            var converter = new Mock<IConverter>(MockBehavior.Strict);
            var expected = new Exception();
            converter.Setup(instance => instance.ConvertTo(typeof(Exception), "test")).Returns(expected);
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Exception), request.Object, true)).Returns(converter.Object);

            var result = _converterProvider.Object.ConvertTo("test", typeof(Exception), request.Object);

            result.Should().Be(expected);
        }

        [Test]
        public void it_should_return_null_when_no_matching_converter_is_found()
        {
            var request = new Mock<IRequestInfo>(MockBehavior.Strict);
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Exception), request.Object, true)).Returns((IConverter)null);

            var result = _converterProvider.Object.ConvertTo("test", typeof(Exception), request.Object);

            result.Should().BeNull();
        }

        [Test]
        public void it_should_serialize_using_type_converters()
        {
            _converterProvider.Object.ConvertFrom(1).Should().Be("1");
        }

        [Test]
        public void it_should_return_null_when_no_value_to_be_serialized_is_provided()
        {
            _converterProvider.Object.ConvertFrom(null).Should().BeNull();
        }

        [Test]
        public void it_should_throw_when_no_converter_provider_is_passed_when_serializing()
        {
            ((IConverterProvider)null).Invoking(instance => instance.ConvertFrom(new Person(), new Mock<IResponseInfo>(MockBehavior.Strict).Object))
                .ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("converterProvider");
        }

        [Test]
        public void it_should_serialize_using_custom_converters()
        {
            var buffer = new MemoryStream();
            var response = new Mock<IResponseInfo>(MockBehavior.Strict);
            response.SetupGet(instance => instance.Body).Returns(new UnclosableStream(buffer));
            var converter = new Mock<IConverter>(MockBehavior.Strict);
            var expected = new Exception();
            converter.Setup(instance => instance.ConvertFrom(typeof(Exception), expected, response.Object));
            _converterProvider.Setup(instance => instance.FindBestOutputConverter(typeof(Exception), response.Object)).Returns(converter.Object);

            _converterProvider.Object.ConvertFrom(expected, response.Object);

            converter.Verify(instance => instance.ConvertFrom(typeof(Exception), expected, response.Object), Times.Once);
        }

        [Test]
        public void it_should_deserialize_an_array_using_type_converters()
        {
            var result = (IEnumerable<int>)_converterProvider.Object.ConvertToCollection(new[] { "1", "2" }, typeof(int[]));

            result.Should().BeEquivalentTo(1, 2);
        }

        [Test]
        public void it_should_return_null_when_no_array_of_values_to_be_deserialized_is_provided()
        {
            _converterProvider.Object.ConvertToCollection(null, typeof(int[])).Should().BeNull();
        }

        [Test]
        public void it_should_throw_when_no_type_while_deserializing_an_array_is_provided()
        {
            _converterProvider.Object.Invoking(instance => instance.ConvertToCollection(new[] { "1", "2" }, null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("collectionType");
        }

        [Test]
        public void it_should_throw_when_no_converter_provider_is_passed_when_deserializing_an_array()
        {
            ((IConverterProvider)null).Invoking(instance => instance.ConvertToCollection(new[] { "1", "2" }, typeof(Exception[]), new Mock<IRequestInfo>(MockBehavior.Strict).Object))
                .ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("converterProvider");
        }

        [Test]
        public void it_should_deserialize_an_array_using_custom_converters()
        {
            var request = new Mock<IRequestInfo>(MockBehavior.Strict);
            var converter = new Mock<IConverter>(MockBehavior.Strict);
            var expected = new Exception();
            converter.Setup(instance => instance.ConvertTo(typeof(Exception), "test")).Returns(expected);
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Exception), request.Object, true)).Returns(converter.Object);

            var result = (IEnumerable<object>)_converterProvider.Object.ConvertToCollection(new[] { "test" }, typeof(Exception[]), request.Object);

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void it_should_return_null_when_no_matching_converter_is_found_when_deserializing_an_array()
        {
            var request = new Mock<IRequestInfo>(MockBehavior.Strict);
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Exception), request.Object, true)).Returns((IConverter)null);

            var result = _converterProvider.Object.ConvertToCollection(new[] { "test" }, typeof(Exception[]), request.Object);

            result.Should().BeNull();
        }

        [SetUp]
        public void Setup()
        {
            _converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
        }

        [TearDown]
        public void Teardown()
        {
            _converterProvider = null;
        }
    }
}