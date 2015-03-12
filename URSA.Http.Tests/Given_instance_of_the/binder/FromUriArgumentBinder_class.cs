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
    public class FromQueryStringArgumentBinder_class : ArgumentBinderTest<FromQueryStringArgumentBinder, FromQueryStringAttribute, int>
    {
        protected override Uri RequestUri { get { return new Uri("http://temp.org/api/test/add?operandA=1&operandB=1"); } }

        protected override Uri MethodUri { get { return new Uri("http://temp.org/api/test/add"); } }

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
            Binder.GetArgumentValue(GetContext());

            Converter.Verify(instance => instance.ConvertTo(It.IsAny<Type>(), "1"), Times.Never);
        }
    }
}