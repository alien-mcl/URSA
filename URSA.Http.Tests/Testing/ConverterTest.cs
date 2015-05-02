using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using URSA.Web.Converters;

namespace URSA.Web.Http.Testing
{
    [ExcludeFromCodeCoverage]
    public abstract class ConverterTest<T, TI> where T : class, IConverter
    {
        protected const string OperationName = "PostStrings";

        protected static readonly Uri BaseUri = new Uri("http://temp.org/api/test/");

        private const string ContentType = "text/plain";

        protected virtual string SingleEntityContentType { get { return ContentType; } }

        protected virtual string MultipleEntitiesContentType { get { return ContentType; } }

        protected virtual string SingleEntityBody { get { return String.Format(CultureInfo.InvariantCulture, "{0}", SingleEntity); } }

        protected virtual string MultipleEntitiesBody { get { return String.Join("\r\n", MultipleEntities.Select(entity => String.Format(CultureInfo.InvariantCulture, "{0}", entity))); } }

        protected abstract TI SingleEntity { get; }

        protected abstract TI[] MultipleEntities { get; }

        protected T Converter { get; private set; }

        [TestMethod]
        public virtual void it_should_test_deserialization_compatibility()
        {
            var result = CanConvertTo<TI>("POST", OperationName, SingleEntityContentType, SingleEntityBody);

            result.Should().NotBe(CompatibilityLevel.None);
        }

        [TestMethod]
        public virtual void it_should_deserialize_message_body_as_an_entity()
        {
            var result = ConvertBodyTo<TI>("POST", OperationName, SingleEntityContentType, SingleEntityBody);
            AssertSingleEntity(result);
        }

        [TestMethod]
        public virtual void it_should_deserialize_message_as_an_entity()
        {
            var result = ConvertTo<TI>("POST", OperationName, SingleEntityContentType, SingleEntityBody);
            AssertSingleEntity(result);
        }

        [TestMethod]
        public virtual void it_should_deserialize_message_body_as_an_array_of_entities()
        {
            var result = ConvertBodyTo<TI[]>("POST", OperationName, MultipleEntitiesContentType, MultipleEntitiesBody);
            AssertMultipleEntities(result);
        }

        [TestMethod]
        public virtual void it_should_deserialize_message_as_an_array_of_entities()
        {
            var result = ConvertTo<TI[]>("POST", OperationName, MultipleEntitiesContentType, MultipleEntitiesBody);
            AssertMultipleEntities(result);
        }

        [TestMethod]
        public virtual void it_should_test_serialization_compatibility()
        {
            var result = CanConvertFrom("POST", OperationName, SingleEntityContentType, SingleEntity);
            
            result.Should().NotBe(CompatibilityLevel.None);
        }

        [TestMethod]
        public virtual void it_should_serialize_an_entity_to_message()
        {
            var content = ConvertFrom("POST", OperationName, SingleEntityContentType, SingleEntity);
            AssertSingleEntityMessage(content);
        }

        [TestMethod]
        public virtual void it_should_serialize_array_of_entities_to_message()
        {
            var content = ConvertFrom("POST", OperationName, MultipleEntitiesContentType, MultipleEntities);
            AssertMultipleEntitiesMessage(content);
        }

        [TestInitialize]
        public void Setup()
        {
            Converter = CreateInstance();
        }

        [TestCleanup]
        public void Teardown()
        {
            Converter = null;
        }

        protected virtual void AssertSingleEntityMessage(string result)
        {
            result.Should().Be(SingleEntityBody);
        }

        protected virtual void AssertMultipleEntitiesMessage(string result)
        {
            result.Should().Be(MultipleEntitiesBody);
        }

        protected virtual void AssertSingleEntity(TI result)
        {
            result.Should().Be(SingleEntity);
        }

        protected virtual void AssertMultipleEntities(TI[] result)
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(MultipleEntities.Length);
            result.First().Should().Be(MultipleEntities.First());
            result.Last().Should().Be(MultipleEntities.Last());
        }

        protected virtual T CreateInstance()
        {
            return (T)typeof(T).GetConstructor(new Type[0]).Invoke(null);
        }

        protected CompatibilityLevel CanConvertFrom<TT>(string method, string handler, string mediaType, TT body)
        {
            return Converter.CanConvertFrom(typeof(TT), MakeResponse(MakeRequest(method, handler, mediaType)));
        }

        protected string ConvertFrom<TT>(string method, string handler, string mediaType, TT body)
        {
            var response = MakeResponse(MakeRequest(method, handler, mediaType));
            Converter.ConvertFrom(typeof(TT), body, response);
            response.Body.Seek(0, SeekOrigin.Begin);
            return new StreamReader(response.Body).ReadToEnd();
        }

        protected TT ConvertBodyTo<TT>(string method, string handler, string mediaType, string body)
        {
            return (TT)Converter.ConvertTo(typeof(TT), body);
        }

        protected TT ConvertTo<TT>(string method, string handler, string mediaType, string body)
        {
            return (TT)Converter.ConvertTo(typeof(TT), MakeRequest(method, handler, mediaType, body));
        }

        protected CompatibilityLevel CanConvertTo<TT>(string method, string handler, string mediaType, string body)
        {
            return Converter.CanConvertTo(typeof(TT), MakeRequest(method, handler, mediaType, body));
        }

        protected RequestInfo MakeRequest(string method, string handler, string mediaType, string body = null)
        {
            var headers = new HeaderCollection { ContentType = mediaType };
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
            return new StringResponseInfo(null, request);
        }
    }
}