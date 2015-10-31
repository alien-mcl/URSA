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
    }
}