using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using URSA.Security;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace URSA.Web.Http.Testing
{
    [ExcludeFromCodeCoverage]
    public abstract class ArgumentBinderTest<T, I, D>
        where T : class, IParameterSourceArgumentBinder<I>
        where I : ParameterSourceAttribute, new()
    {
        protected Mock<IConverterProvider> ConverterProvider { get; private set; }

        protected Mock<IConverter> Converter { get; private set; }

        protected T Binder { get; private set; }

        protected abstract HttpUrl RequestUrl { get; }

        protected abstract HttpUrl MethodUrl { get; }

        protected abstract string MethodName { get; }

        [SetUp]
        public void Setup()
        {
            SetupConverter(Converter = new Mock<IConverter>(MockBehavior.Strict));
            SetupConverterProvider(ConverterProvider = new Mock<IConverterProvider>(MockBehavior.Strict));
            Binder = CreateBinderInstance(ConverterProvider.Object);
        }

        [TearDown]
        public void Teardown()
        {
            Converter = null;
            ConverterProvider = null;
            Binder = null;
        }

        protected virtual void SetupConverter(Mock<IConverter> converter)
        {
            converter.Setup(instance => instance.CanConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()))
                .Returns<Type, IRequestInfo>((type, request) => CompatibilityLevel.ExactMatch);
            converter.Setup(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()))
                .Returns<Type, IRequestInfo>((type, body) => default(D).ToString());
            converter.Setup(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns<Type, string>((type, body) => default(D).ToString());
        }

        protected virtual void SetupConverterProvider(Mock<IConverterProvider> converterProvider)
        {
            converterProvider.Setup(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), false))
                .Returns<Type, IRequestInfo, bool>((type, request, ignoreProtocol) => Converter.Object);
            converterProvider.Setup(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), true))
                .Returns<Type, IRequestInfo, bool>((type, request, ignoreProtocol) => Converter.Object);
        }

        protected ArgumentBindingContext<I> GetContext(string body = null, string verb = "GET", string contentType = null, string multipartBoundary = null)
        {
            return GetContext((body != null ? Encoding.UTF8.GetBytes(body) : null), verb, contentType, multipartBoundary);
        }

        protected ArgumentBindingContext<I> GetContext(byte[] body, string verb = "GET", string contentType = null, string multipartBoundary = null)
        {
            var httpVerb = Verb.Parse(verb);
            var method = typeof(TestController).GetMethod(MethodName);
            var headers = new HeaderCollection();
            if ((body != null) && (body.Length > 0))
            {
                headers.ContentType = (String.IsNullOrEmpty(multipartBoundary) ? (!String.IsNullOrEmpty(contentType) ? contentType : "text/plan") :
                    (!String.IsNullOrEmpty(contentType) ? contentType : "multipart/mixed") + "; boundary=\"" + multipartBoundary + "\"");
            }

            var operation = new OperationInfo<Verb>(
                method,
                MethodUrl.AsRelative,
                MethodUrl.ToString(),
                new Regex(".*"),
                httpVerb,
                method.GetParameters().Select(item => new ArgumentInfo(item, new I(), "test={?test}", "test")).ToArray());
            return (ArgumentBindingContext<I>)typeof(ArgumentBindingContext<I>)
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .First()
                .Invoke(new object[]
                    {
                        new RequestInfo(httpVerb, RequestUrl, (body != null ? new MemoryStream(body) : new MemoryStream()), new BasicClaimBasedIdentity(), headers),
                        new RequestMapping(new TestController(), operation, MethodUrl),
                        method.GetParameters().FirstOrDefault(item => item.GetCustomAttribute<I>(true) != null) ?? method.GetParameters()[0],
                        0,
                        new I(),
                        new Dictionary<RequestInfo, RequestInfo[]>()
                    });
        }

        private static T CreateBinderInstance(IConverterProvider converterProvider)
        {
            return (T)typeof(T)
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .First()
                .Invoke(new object[] { converterProvider });
        }
    }
}