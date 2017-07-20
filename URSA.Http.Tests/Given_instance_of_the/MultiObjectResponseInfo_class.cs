using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA;
using URSA.Security;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class MultiObjectResponseInfo_class : IDisposable
    {
        private const string StringBody = "test";
        private const int NumericBody = 1;
        private static readonly string StringMessageTemplate = String.Format("--.+\r\nContent-Type:text/plain\r\nContent-Length:{1}\r\n\r\n{0}\r\n", StringBody, Encoding.UTF8.GetByteCount(StringBody));
        private static readonly string NumericMessageTemplate = String.Format("--.+\r\nContent-Type:text/plain\r\nContent-Length:{1}\r\n\r\n{0}\r\n", NumericBody, Encoding.UTF8.GetByteCount(NumericBody.ToString()));
        private static readonly string MessageTemplate = String.Format("{0}{1}--.+--", StringMessageTemplate, NumericMessageTemplate);
        private Mock<IConverter> _converter;
        private Mock<IConverterProvider> _converterProvider;
        private RequestInfo _request;
        private MultiObjectResponseInfo _response;

        [Test]
        public void it_should_call_converter_provider()
        {
            _converterProvider.Verify(instance => instance.FindBestOutputConverter<string>(It.IsAny<ObjectResponseInfo<string>>()), Times.Once);
            _converterProvider.Verify(instance => instance.FindBestOutputConverter<int>(It.IsAny<ObjectResponseInfo<int>>()), Times.Once);
        }

        [Test]
        public void it_should_serialize_responses()
        {
            _converter.Verify(instance => instance.ConvertFrom(StringBody, It.IsAny<ObjectResponseInfo<string>>()), Times.Once);
            _converter.Verify(instance => instance.ConvertFrom(NumericBody, It.IsAny<ObjectResponseInfo<int>>()), Times.Once);
        }

        [Test]
        public void it_should_serialize_bodies()
        {
            new StreamReader(_response.Body).ReadToEnd().Should().MatchRegex(MessageTemplate);
        }

        [SetUp]
        public void Setup()
        {
            (_converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict)).Setup(instance => instance.FindBestOutputConverter<string>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => _converter.Object);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<int>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => _converter.Object);
            (_converter = new Mock<IConverter>(MockBehavior.Strict)).Setup(instance => instance.CanConvertFrom<string>(It.IsAny<IResponseInfo>()))
                .Returns<IResponseInfo>(response => CompatibilityLevel.ExactMatch);
            _converter.Setup(instance => instance.ConvertFrom(StringBody, It.IsAny<IResponseInfo>()))
                .Callback<string, IResponseInfo>((body, response) =>
                {
                    response.Headers["Content-Type"] = "text/plain";
                    using (var writer = new StreamWriter(response.Body))
                    {
                        writer.Write(body);
                        writer.Flush();
                    }
                });
            _converter.Setup(instance => instance.ConvertFrom(NumericBody, It.IsAny<IResponseInfo>()))
                .Callback<int, IResponseInfo>((body, response) =>
                {
                    response.Headers["Content-Type"] = "text/plain";
                    using (var writer = new StreamWriter(response.Body))
                    {
                        writer.Write(body);
                        writer.Flush();
                    }
                });
            _request = new RequestInfo(Verb.GET, (HttpUrl)UrlParser.Parse("http://temp.org/"), new MemoryStream(), new BasicClaimBasedIdentity());
            _response = new MultiObjectResponseInfo(_request, new object[] { StringBody, NumericBody }, _converterProvider.Object);
        }

        [TearDown]
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