using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using URSA.Security;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Testing;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the.binder
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FromUriArgumentBinder_class : ArgumentBinderTest<FromUriArgumentBinder, FromUriAttribute, int>
    {
        protected override Uri RequestUri { get { return new Uri("http://temp.org/api/test/sub/1?operandB=1"); } }

        protected override Uri MethodUri { get { return new Uri("http://temp.org/api/test/sub"); } }

        protected override string MethodName { get { return "Substract"; } }

        [TestMethod]
        public void it_should_not_call_converter_provider()
        {
            Binder.GetArgumentValue(GetContext());

            ConverterProvider.Verify(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), true), Times.Never);
        }

        [TestMethod]
        public void it_should_not_call_converter()
        {
            Binder.GetArgumentValue((ArgumentBindingContext)GetContext());

            Converter.Verify(instance => instance.ConvertTo(It.IsAny<Type>(), "1"), Times.Never);
        }

        [TestMethod]
        public void it_should_throw_when_parameter_type_is_an_enumeration()
        {
            var method = typeof(TestController).GetMethod("Collection");
            var context = new ArgumentBindingContext<FromUriAttribute>(
                new RequestInfo(Verb.GET, RequestUri, new MemoryStream(), new BasicClaimBasedIdentity()),
                new RequestMapping(new TestController(), method.ToOperationInfo("/", Verb.GET), new Uri("/", UriKind.Relative)),
                method.GetParameters()[0],
                1,
                new FromUriAttribute(),
                new Dictionary<RequestInfo, RequestInfo[]>());

            Binder.Invoking(instance => instance.GetArgumentValue(context)).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void it_should_throw_when_no_binding_context_is_given()
        {
            Binder.Invoking(instance => instance.GetArgumentValue(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }
    }
}