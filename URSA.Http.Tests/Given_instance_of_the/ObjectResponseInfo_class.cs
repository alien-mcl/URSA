using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using URSA.Web.Converters;

namespace URSA.Web.Http.Tests.Given_instance_of_the
{
    [TestClass]
    public class ObjectResponseInfo_class
    {
        [TestMethod]
        public void it_should_call_converter_provider()
        {
            Mock<IConverter> converter = new Mock<IConverter>();
            converter.Setup(instance => instance.CanConvertFrom<string>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => CompatibilityLevel.ExactMatch);
            Mock<IConverterProvider> converterProvider = new Mock<IConverterProvider>();
            converterProvider.Setup(instance => instance.FindBestOutputConverter<string>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => converter.Object);
            RequestInfo requestInfo = new RequestInfo(Verb.GET, new Uri("http://temp.org/"), new MemoryStream());
            var responseInfo = new ObjectResponseInfo<string>(requestInfo, "test", converterProvider.Object);

            converterProvider.Verify(instance => instance.FindBestOutputConverter<string>(responseInfo), Times.Once);
        }
    }
}