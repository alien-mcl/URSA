﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using FluentAssertions;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Data;
using URSA.Web.Mapping;

namespace Given_instance_of_the
{
    [TestClass]
    public class Client_class
    {
        private const string RelativeUri = "/test/person/{?id}";
        private static readonly Uri CallUri = new Uri("http://temp.uri/");
        private static readonly dynamic Arguments = new ExpandoObject();
        private static readonly Person Person = new Person() { Id = 1, FirstName = "test", LastName = "test" };
        private Mock<IComponentProvider> _container;
        private Mock<HttpWebResponse> _webResponse;
        private Mock<HttpWebRequest> _webRequest;
        private Mock<IWebRequestProvider> _webRequestProvider;
        private Mock<IConverterProvider> _converterProvider;
        private Mock<IConverter> _converter;
        private Client _client;

        [TestMethod]
        public void it_should_build_a_request_url()
        {
            Arguments.id = 1;

            Uri result = _client.BuildUri(RelativeUri, Arguments);

            result.Should().Be(new Uri(CallUri.AbsoluteUri + RelativeUri.Substring(1).Replace("{?id}", "1")));
        }

        [TestMethod]
        public void it_should_create_a_request()
        {
            Call();

            _webRequestProvider.Verify(instance => instance.SupportedProtocols, Times.Once);
            _webRequestProvider.Verify(instance => instance.CreateRequest(It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
        }

        [TestMethod]
        public void it_should_serialize_request_body()
        {
            Call();

            _converterProvider.Verify(instance => instance.FindBestOutputConverter<Person>(It.IsAny<IResponseInfo>()), Times.Once);
            _converter.Verify(instance => instance.ConvertFrom(Person, It.IsAny<IResponseInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_deserialize_response_body()
        {
            Call();

            _converterProvider.Verify(instance => instance.FindBestInputConverter(typeof(Person), It.IsAny<IRequestInfo>(), false), Times.Once);
            _converter.Verify(instance => instance.ConvertTo(typeof(Person), It.IsAny<IRequestInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_return_result()
        {
            var result = Call();

            result.Should().Be(Person);
        }

        [TestInitialize]
        public void Setup()
        {
            var headers = new WebHeaderCollection { { "Content-Type", "application/json" } };
            _webResponse = new Mock<HttpWebResponse>();
            _webResponse.Setup(instance => instance.GetResponseStream()).Returns(new MemoryStream());
            _webResponse.SetupGet(instance => instance.Headers).Returns(headers);
            _webRequest = new Mock<HttpWebRequest>();
            _webRequest.Setup(instance => instance.GetRequestStream()).Returns(new MemoryStream());
            _webRequest.Setup(instance => instance.GetResponse()).Returns(_webResponse.Object);
            _webRequestProvider = new Mock<IWebRequestProvider>(MockBehavior.Strict);
            _webRequestProvider.SetupGet(instance => instance.SupportedProtocols).Returns(new[] { "http" });
            _webRequestProvider.Setup(instance => instance.CreateRequest(It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>())).Returns(_webRequest.Object);
            _converter = new Mock<IConverter>(MockBehavior.Strict);
            _converter.Setup(instance => instance.ConvertFrom(Person, It.IsAny<IResponseInfo>()));
            _converter.Setup(instance => instance.ConvertTo(typeof(Person), It.IsAny<IRequestInfo>())).Returns(Person);
            _converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            _converterProvider.Setup(instance => instance.FindBestOutputConverter<Person>(It.IsAny<IResponseInfo>())).Returns(_converter.Object);
            _converterProvider.Setup(instance => instance.FindBestInputConverter(typeof(Person), It.IsAny<IRequestInfo>(), false)).Returns(_converter.Object);
            UrsaConfigurationSection.ComponentProvider = (_container = new Mock<IComponentProvider>(MockBehavior.Strict)).Object;
            _container.Setup(instance => instance.Resolve<IConverterProvider>()).Returns(_converterProvider.Object);
            _container.Setup(instance => instance.ResolveAll<IWebRequestProvider>()).Returns(new[] { _webRequestProvider.Object });
            _client = new Client(CallUri);
        }

        [TestCleanup]
        public void Teardown()
        {
            _webRequestProvider = null;
            _container = null;
            _client = null;
        }

        private Person Call()
        {
            Arguments.id = 1;
            return _client.Call<Person>(Verb.PUT, RelativeUri, new[] { "application/json" }, Arguments, Person);
        }
    }
}