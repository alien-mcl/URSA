using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
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

        protected abstract Uri RequestUri { get; }

        protected abstract Uri MethodUri { get; }

        protected abstract string MethodName { get; }

        [TestInitialize]
        public void Setup()
        {
            Converter = new Mock<IConverter>(MockBehavior.Strict);
            Converter.Setup(instance => instance.CanConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()))
                .Returns<Type, IRequestInfo>((type, request) => CompatibilityLevel.ExactMatch);
            Converter.Setup(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<IRequestInfo>()))
                .Returns<Type, IRequestInfo>((type, body) => default(D).ToString());
            Converter.Setup(instance => instance.ConvertTo(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns<Type, string>((type, body) => default(D).ToString());
            ConverterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            ConverterProvider.Setup(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), false))
                .Returns<Type, IRequestInfo, bool>((type, request, ignoreProtocol) => Converter.Object);
            ConverterProvider.Setup(instance => instance.FindBestInputConverter(It.IsAny<Type>(), It.IsAny<IRequestInfo>(), true))
                .Returns<Type, IRequestInfo, bool>((type, request, ignoreProtocol) => Converter.Object);
            Binder = CreateBinderInstance(ConverterProvider.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            Converter = null;
            ConverterProvider = null;
            Binder = null;
        }

        protected ArgumentBindingContext<I> GetContext(string verb = "GET", string body = null, string multipartBoundary = null)
        {
            var httpVerb = Verb.Parse(verb);
            var method = typeof(TestController).GetMethod(MethodName);
            var headers = new HeaderCollection();
            if ((!String.IsNullOrEmpty(body)) && (!String.IsNullOrEmpty(multipartBoundary)))
            {
                headers.ContentType = "multipart/mixed; boundary=\"" + multipartBoundary + "\"";
            }

            var operation = new OperationInfo<Verb>(
                method,
                MethodUri.ToRelativeUri(),
                MethodUri.ToString(),
                new Regex(".*"),
                httpVerb,
                method.GetParameters().Select(item => new ArgumentInfo(item, new I(), "test={?test}", "test")).ToArray());
            return (ArgumentBindingContext<I>)typeof(ArgumentBindingContext<I>)
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .First()
                .Invoke(new object[]
                    {
                        new RequestInfo(httpVerb, RequestUri, (body != null ? new MemoryStream(Encoding.UTF8.GetBytes(body)) : new MemoryStream()), headers),
                        new RequestMapping(new TestController(), operation, MethodUri),
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