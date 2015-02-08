using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using URSA.Web.Converters;
using URSA.Web.Tests;

namespace URSA.Web.Http.Testing
{
    public abstract class ConverterTest<T> where T : class, IConverter
    {
        protected static readonly Uri BaseUri = new Uri("http://temp.org/api/test/");

        private static readonly Type ControllerType = typeof(TestController);

        protected T Converter { get; private set; }

        protected Mock<IConverterProvider> ConverterProvider { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            ConverterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            Converter = CreateInstance();
        }

        [TestCleanup]
        public void Teardown()
        {
            Converter = null;
        }

        protected virtual T CreateInstance()
        {
            return (T)typeof(T).GetConstructor(new Type[0]).Invoke(null);
        }

        protected string ConvertFrom<TT>(string method, string handler, string mediaType, TT body)
        {
            var headers = new HeaderCollection();
            var response = MakeResponse(MakeRequest(method, handler, mediaType));
            ConverterProvider.Setup(instance => instance.FindBestOutputConverter<TT>(It.IsAny<ResponseInfo>()))
                .Returns<IResponseInfo>(responseInfo => Converter);
            return new StreamReader(new ObjectResponseInfo<TT>(response.Request, body, ConverterProvider.Object).Body).ReadToEnd();
        }

        protected TT ConvertTo<TT>(string method, string handler, string mediaType, string body)
        {
            return Converter.ConvertTo<TT>(MakeRequest(method, handler, mediaType, body));
        }

        protected RequestInfo MakeRequest(string method, string handler, string mediaType, string body = null)
        {
            var headers = new HeaderCollection();
            headers.ContentType = mediaType;
            if (body != null)
            {
                headers.ContentLength = body.Length;
            }

            headers.Accept = mediaType;
            return new RequestInfo(
                Verb.Parse(method),
                new Uri(BaseUri, handler),
                (body != null ? new MemoryStream(Encoding.UTF8.GetBytes(body)) : new MemoryStream()),
                headers);
        }

        protected ResponseInfo MakeResponse(RequestInfo request)
        {
            return new StringResponseInfo(String.Empty, request);
        }
    }
}