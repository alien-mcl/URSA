using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using URSA;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Testing;
using URSA.Web.Mapping;

namespace Given_instance_of_the.binder
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FromQueryStringArgumentBinder_class : ArgumentBinderTest<FromQueryStringArgumentBinder, FromQueryStringAttribute, int>
    {
        private HttpUrl _requestUrl = (HttpUrl)UrlParser.Parse("http://temp.org/api/test/add?operandA=1&operandB=1");

        protected override HttpUrl RequestUrl { get { return _requestUrl; } }

        protected override HttpUrl MethodUrl { get { return (HttpUrl)UrlParser.Parse("http://temp.org/api/test/add"); } }

        protected override string MethodName { get { return "Add"; } }

        [TestMethod]
        public void it_should_not_call_converter_provider()
        {
            Binder.GetArgumentValue(GetContext());

            ConverterProvider.Verify(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), false), Times.Never);
        }

        [TestMethod]
        public void it_should_not_call_converter()
        {
            Binder.GetArgumentValue((ArgumentBindingContext)GetContext());

            Converter.Verify(instance => instance.ConvertTo(It.IsAny<Type>(), "1"), Times.Never);
        }

        [TestMethod]
        public void it_should_throw_when_no_binding_context_is_given()
        {
            Binder.Invoking(instance => instance.GetArgumentValue(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [TestMethod]
        public void it_should_return_null_if_the_query_string_is_to_short_to_contain_any_valid_value()
        {
            _requestUrl = (HttpUrl)UrlParser.Parse("http://temp.org/api/test/add?_");
            var context = (ArgumentBindingContext)GetContext();

            var result = Binder.GetArgumentValue(context);

            result.Should().BeNull();
        }
    }
}