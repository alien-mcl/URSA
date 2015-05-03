using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Testing;
using URSA.Web.Mapping;

namespace Given_instance_of_the.binder
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FromBodyArgumentBinder_class : ArgumentBinderTest<FromBodyArgumentBinder, FromBodyAttribute, int>
    {
        private const string Boundary = "test";
        private const string Body =
            "--" + Boundary + "\r\nContent-Type: text/plain\r\nContent-Length:3\r\n\r\n1\r\n" +
            "--" + Boundary + "\r\nContent-Type: text/plain\r\nContent-Length:3\r\n\r\n2\r\n--" + Boundary + "--";

        protected override Uri RequestUri { get { return new Uri("http://temp.org/api/test/modulo"); } }

        protected override Uri MethodUri { get { return new Uri("http://temp.org/api/test/modulo"); } }

        protected override string MethodName { get { return "PostModulo"; } }

        [TestMethod]
        public void it_should_call_converter_provider()
        {
            Binder.GetArgumentValue(GetContext("POST", Body, Boundary));

            ConverterProvider.Verify(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), false), Times.Once);
        }

        [TestMethod]
        public void it_should_call_converter()
        {
            Binder.GetArgumentValue((ArgumentBindingContext)GetContext("POST", Body, Boundary));

            Converter.Verify(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()), Times.Once);
        }
    }
}