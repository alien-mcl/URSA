using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using URSA;
using URSA.Security;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Mapping;

namespace Given_instance_of_the.ResponseComposer_class
{
    public class ResponseComposerTest<T> where T : IController, new()
    {
        protected Mock<IConverter> Converter { get; private set; }

        protected Mock<IConverterProvider> ConverterProvider { get; private set; }

        protected Mock<IHttpControllerDescriptionBuilder<T>> ControllerDescriptionBuilder { get; private set; }

        protected T Controller { get; private set; }

        protected ResponseComposer Composer { get; private set; }

        [SetUp]
        public void Setup()
        {
            Converter = new Mock<IConverter>(MockBehavior.Strict);
            ConverterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            ConverterProvider.Setup(instance => instance.FindBestOutputConverter<int>(It.IsAny<IResponseInfo>())).Returns(Converter.Object);
            ConverterProvider.Setup(instance => instance.FindBestOutputConverter<double>(It.IsAny<IResponseInfo>())).Returns(Converter.Object);
            ConverterProvider.Setup(instance => instance.FindBestOutputConverter<string>(It.IsAny<IResponseInfo>())).Returns(Converter.Object);
            ConverterProvider.Setup(instance => instance.FindBestOutputConverter<Guid>(It.IsAny<IResponseInfo>())).Returns(Converter.Object);
            ConverterProvider.Setup(instance => instance.FindBestOutputConverter<object>(It.IsAny<IResponseInfo>())).Returns(Converter.Object);
            ControllerDescriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<T>>(MockBehavior.Strict);
            Controller = new T() { Response = new StringResponseInfo(Encoding.UTF8, null, new RequestInfo(Verb.GET, (HttpUrl)UrlParser.Parse("http://temp.uri/api"), new MemoryStream(), new BasicClaimBasedIdentity())) };
            Composer = new ResponseComposer(ConverterProvider.Object, new[] { ControllerDescriptionBuilder.Object });
        }

        [TearDown]
        public void Teardown()
        {
            Composer = null;
            ConverterProvider = null;
        }

        protected RequestMapping CreateRequestMapping(string methodName, params object[] arguments)
        {
            return CreateRequestMapping(methodName, Verb.GET, arguments);
        }

        protected RequestMapping CreateRequestMapping(string methodName, Verb httpMethod, params object[] arguments)
        {
            var method = Controller.GetType().GetMethod(methodName);
            string baseUri = Controller.GetType().GetTypeInfo().GetCustomAttribute<RouteAttribute>().Url.ToString();
            string callUri;
            return new RequestMapping(Controller, method.ToOperationInfo(baseUri, httpMethod, out callUri, arguments), (HttpUrl)UrlParser.Parse(callUri.TrimStart('/')));
        }
    }
}