using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Testing;
using URSA.Web.Mapping;

namespace Given_instance_of_the.binder.FromBodyArgumentBinder_class
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class when_binding_to_X_WWW_Url_Encoded_body : ArgumentBinderTest<FromBodyArgumentBinder, FromBodyAttribute, int>
    {
        private const string Body = "operandA=1&operandB=2";

        protected override HttpUrl RequestUrl { get { return (HttpUrl)UrlParser.Parse("http://temp.org/api/test/modulo"); } }

        protected override HttpUrl MethodUrl { get { return (HttpUrl)UrlParser.Parse("http://temp.org/api/test/modulo"); } }

        protected override string MethodName { get { return "PostModulo"; } }

        [Test]
        public void it_should_call_converter_provider()
        {
            Binder.GetArgumentValue(GetContext(Body, "POST", "application/x-www-url-encoded"));

            ConverterProvider.Verify(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), false), Times.Once);
        }

        [Test]
        public void it_should_not_call_converter()
        {
            Binder.GetArgumentValue((ArgumentBindingContext)GetContext(Body, "POST", "application/x-www-url-encoded"));

            Converter.Verify(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()), Times.Never);
        }

        [Test]
        public void it_should_provide_a_value()
        {
            var result = Binder.GetArgumentValue((ArgumentBindingContext)GetContext(Body, "POST", "application/x-www-url-encoded"));

            result.Should().Be(1);
        }

        [Test]
        public void it_should_throw_when_no_binding_context_is_given()
        {
            Binder.Invoking(instance => instance.GetArgumentValue(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        protected override void SetupConverter(Mock<IConverter> converter)
        {
            converter.Setup(instance => instance.CanConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()))
                .Returns<Type, IRequestInfo>((type, request) => CompatibilityLevel.ExactProtocolMatch);
        }
    }
}
