using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using URSA;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Tests.Data;

namespace Given_instance_of_the
{
    [TestClass]
    public class Client_class
    {
        private const string AuthenticationScheme = "Basic";
        private const string RelativeUri = "/test/person/{id}";
        private static readonly HttpUrl CallUrl = (HttpUrl)UrlParser.Parse("http://temp.uri/");
        private static readonly Person Person = new Person() { Key = 1, FirstName = "test", LastName = "test" };

        private dynamic _arguments;
        private Mock<IComponentProvider> _container;
        private Mock<HttpWebResponse> _webResponse;
        private Mock<HttpWebRequest> _webRequest;
        private Mock<IWebRequestProvider> _webRequestProvider;
        private Mock<IConverterProvider> _converterProvider;
        private Mock<IConverter> _converter;
        private Mock<IResultBinder<RequestInfo>> _resultBinder;
        private WebHeaderCollection _headers;
        private MemoryStream _requestStream;
        private Client _client;

        private enum With
        {
            StronglyTypedResult,
            NoResult,
            MultipartBody
        }

        [TestMethod]
        public void it_should_build_a_request_url()
        {
            _arguments.id = 1;

            HttpUrl result = _client.BuildUrl(RelativeUri, _arguments);

            result.Should().Be((HttpUrl)CallUrl.AddSegments(RelativeUri.Replace("{id}", "1").Split('/')));
        }

        [TestMethod]
        public async Task it_should_create_a_request()
        {
            await Call();

            _webRequestProvider.Verify(instance => instance.SupportedProtocols, Times.Once);
            _webRequestProvider.Verify(instance => instance.CreateRequest(It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task it_should_use_credentials_available_in_cache()
        {
            var expectedUserName = "userName";
            var expectedPassword = "password";
            CredentialCache.DefaultNetworkCredentials.UserName = expectedUserName;
            CredentialCache.DefaultNetworkCredentials.Password = expectedPassword;
            var url = (HttpUrl)CallUrl.AddSegments(RelativeUri.Split('/'));

            await Call();

            _webRequest.VerifySet(
                instance => instance.Credentials = It.Is<ICredentials>(
                    credentials => Test(credentials, url, expectedUserName, expectedPassword)),
                Times.Once);
        }

        [TestMethod]
        public async Task it_should_serialize_request_body()
        {
            await Call();

            _converterProvider.Verify(instance => instance.FindBestOutputConverter<Person>(It.IsAny<IResponseInfo>()), Times.Once);
            _converter.Verify(instance => instance.ConvertFrom(Person, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public async Task it_should_deserialize_response_body()
        {
            await Call();

            _resultBinder.Verify(instance => instance.BindResults(typeof(Person), It.IsAny<RequestInfo>()), Times.Once);
        }

        [TestMethod]
        public async Task it_should_return_result()
        {
            var result = await Call();

            result.Should().Be(Person);
        }

        [TestMethod]
        public async Task it_should_return_untyped_result()
        {
            await Call(With.NoResult);

            _webRequest.Verify(instance => instance.GetResponseAsync(), Times.Once);
        }

        [TestMethod]
        public async Task it_should_parse_Content_Range_headers()
        {
            await Call();

            ((int)_arguments.totalEntities).Should().Be(1);
        }

        [TestMethod]
        public void it_should_throw_when_Content_Range_header_is_incorrectly_formatted()
        {
            _headers["Content-Range"] = "test";

            ((Client)null).Awaiting(_ => Call()).ShouldThrow<FormatException>();
        }

        [TestMethod]
        public async Task it_should_deserialize_multipart_body_correctly()
        {
            await Call(With.MultipartBody);

            _requestStream.Seek(0, SeekOrigin.Begin);
            Encoding.UTF8.GetString(_requestStream.ToArray()).Should().Contain("--");
        }

        [TestInitialize]
        public void Setup()
        {
            _arguments = new ExpandoObject();
            _headers = new WebHeaderCollection();
            _headers["Content-Type"] = "application/json";
            _headers["Content-Length"] = "0";
            _headers["Content-Range"] = " 0-0/1";
            _webResponse = new Mock<HttpWebResponse>();
            _webResponse.Setup(instance => instance.GetResponseStream()).Returns(new MemoryStream());
            _webResponse.SetupGet(instance => instance.Headers).Returns(_headers);
            var webHeaders = new WebHeaderCollection();
            _webRequest = new Mock<HttpWebRequest>();
            _webRequest.Setup(instance => instance.GetRequestStreamAsync()).ReturnsAsync(new UnclosableStream(_requestStream = new MemoryStream()));
            _webRequest.Setup(instance => instance.GetResponseAsync()).ReturnsAsync(_webResponse.Object);
            _webRequest.SetupSet(instance => instance.Credentials = It.IsAny<ICredentials>());
            _webRequest.SetupGet(instance => instance.Headers).Returns(webHeaders);
            _webRequestProvider = new Mock<IWebRequestProvider>(MockBehavior.Strict);
            _webRequestProvider.SetupGet(instance => instance.SupportedProtocols).Returns(new[] { "http" });
            _webRequestProvider.Setup(instance => instance.CreateRequest(It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>())).Returns(_webRequest.Object);
            _converter = new Mock<IConverter>(MockBehavior.Strict);
            _converter.Setup(instance => instance.ConvertFrom(Person, It.IsAny<IResponseInfo>())).Callback<Person, IResponseInfo>((instance, response) =>
                {
                    response.Headers["Content-Length"] = "0";
                    response.Headers["Content-Range"] = "0-0/1";
                    response.Headers["Content-Type"] = "application/json";
                });
            _converter.Setup(instance => instance.ConvertTo(typeof(Person), It.IsAny<IRequestInfo>())).Returns(Person);
            _converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            _converterProvider.SetupGet(instance => instance.SupportedMediaTypes).Returns(new[] { "application/json" });
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<Person>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Person), It.IsAny<IRequestInfo>(), false)).Returns(_converter.Object);
            _resultBinder = new Mock<IResultBinder<RequestInfo>>(MockBehavior.Strict);
            _resultBinder.Setup(instance => instance.BindResults(It.IsAny<Type>(), It.IsAny<RequestInfo>())).Returns(new object[] { Person });
            UrsaConfigurationSection.ComponentProvider = (_container = new Mock<IComponentProvider>(MockBehavior.Strict)).Object;
            _container.Setup(instance => instance.Resolve<IConverterProvider>(null)).Returns(_converterProvider.Object);
            _container.Setup(instance => instance.ResolveAll<IWebRequestProvider>(null)).Returns(new[] { _webRequestProvider.Object });
            _container.Setup(instance => instance.Resolve<IResultBinder<RequestInfo>>(null)).Returns(_resultBinder.Object);
            _container.Setup(instance => instance.ResolveAll<IRequestModelTransformer>(null)).Returns(new IRequestModelTransformer[0]);
            _client = new Client(CallUrl, AuthenticationScheme);
        }

        [TestCleanup]
        public void Teardown()
        {
            _arguments = null;
            _webRequestProvider = null;
            _webRequest = null;
            _webResponse = null;
            _converter = null;
            _converterProvider = null;
            _headers = null;
            _container = null;
            _client = null;
        }

        private async Task<Person> Call(With options = With.StronglyTypedResult)
        {
            _arguments.id = 1;
            if (options == With.NoResult)
            {
                _client.Call(Verb.PUT, RelativeUri, new[] { "application/json" }, new[] { "application/json" }, _arguments, Person);
                return null;
            }

            if (options == With.MultipartBody)
            {
                return await _client.Call<Person>(Verb.PUT, RelativeUri, new[] { "application/json" }, new[] { "application/json" }, _arguments, Person, Person);
            }

            return await _client.Call<Person>(Verb.PUT, RelativeUri, new[] { "application/json" }, new[] { "application/json" }, _arguments, Person);
        }

        private bool Test(ICredentials credentials, HttpUrl url, string expectedUserName, string expectedPassword)
        {
            Uri uri = new Uri(String.Format("{0}://{1}/", url.Scheme, url.Host));
            return (credentials.GetCredential(uri, AuthenticationScheme).UserName == expectedUserName) &&
                (credentials.GetCredential(uri, AuthenticationScheme).Password == expectedPassword);
        }
    }
}