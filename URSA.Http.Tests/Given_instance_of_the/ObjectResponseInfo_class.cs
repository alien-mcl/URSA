using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using URSA.Security;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ObjectResponseInfo_class : IDisposable
    {
        private const string Body = "test";
        private Mock<IConverter> _converter;
        private Mock<IConverterProvider> _converterProvider;
        private RequestInfo _request;
        private ObjectResponseInfo<string> _response;
        
        [TestMethod]
        public void it_should_call_converter_provider()
        {
            _converterProvider.Verify(instance => instance.FindBestOutputConverter<string>(_response), Times.Once);
        }

        [TestMethod]
        public void it_should_serialize_response()
        {
            _converter.Verify(instance => instance.ConvertFrom(Body, _response), Times.Once);
        }

        [TestMethod]
        public void it_should_serialize_body()
        {
            new StreamReader(_response.Body).ReadToEnd().Should().Be(Body);
        }

        [TestInitialize]
        public void Setup()
        {
            (_converter = new Mock<IConverter>(MockBehavior.Strict)).Setup(instance => instance.CanConvertFrom<string>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => CompatibilityLevel.ExactMatch);
            (_converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict)).Setup(instance => instance.FindBestOutputConverter<string>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => _converter.Object);
            _converter.Setup(instance => instance.ConvertFrom(Body, It.IsAny<ObjectResponseInfo<string>>()))
                .Callback<string, IResponseInfo>((body, response) =>
                {
                    using (var writer = new StreamWriter(response.Body))
                    {
                        writer.Write(body);
                        writer.Flush();
                    }
                });
            _request = new RequestInfo(Verb.GET, new Uri("http://temp.org/"), new MemoryStream(), new BasicClaimBasedIdentity());
            _response = new ObjectResponseInfo<string>(_request, Body, _converterProvider.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            _converter = null;
            _converterProvider = null;
            _request = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_request != null)
            {
                _request.Dispose();
            }

            if (_response != null)
            {
                _response.Dispose();
            }
        }
    }
}