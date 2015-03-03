using RomanticWeb;
using RomanticWeb.ComponentModel;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using URSA.Web.Converters;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts entities from and to RDF serialization.</summary>
    public class EntityConverter : IConverter
    {
        /// <summary>Defines a document name for <![CDATA[XML/XSLT]]> documentation style-sheet.</summary>
        public const string DocumentationStylesheet = "documentation-stylesheet";

        /// <summary>Defines a '<![CDATA[text/turtle]]>' media type.</summary>
        public const string TextTurtle = "text/turtle";

        /// <summary>Defines a '<![CDATA[application/rdf+xml]]>' media type.</summary>
        public const string ApplicationRdfXml = "application/rdf+xml";

        /// <summary>Defines a '<![CDATA[application/owl+xml]]>' media type.</summary>
        public const string ApplicationOwlXml = "application/owl+xml";

        /// <summary>Defines a '<![CDATA[application/ld+json]]>' media type.</summary>
        public const string ApplicationLdJson = "application/ld+json";

        private static readonly string[] SupportedMediaTypes = new[] { TextTurtle, ApplicationRdfXml, ApplicationOwlXml, ApplicationLdJson };
        private static readonly Uri Hydra = new Uri("http://www.w3.org/ns/hydra/core#");
        private static readonly string Context = String.Format(
            @"{{
                ""rdfs"": ""{0}"",
                ""hydra"": ""{1}"",
                ""label"": ""rdfs:label"",
                ""comment"": ""rdfs:comment"",
                ""range"": ""rdfs:range"",
                ""domain"": ""rdfs:domain"",
                ""subClassOf"": ""rdfs:subClassOf"",
                ""title"": ""hydra:title"",
                ""description"": ""hydra:description"",
                ""supportedClasses"": ""hydra:supportedClasses"",
                ""supportedProperties"": ""hydra:supportedProperties"",
                ""supportedOperation"": ""hydra:supportedOperation"",
                ""operation"": ""hydra:operation"",
                ""statusCodes"": ""hydra:statusCodes"",
                ""entrypoint"": ""hydra:entrypoint"",
                ""template"": ""hydra:template"",
                ""mapping"": ""hydra:mapping"",
                ""variable"": ""hydra:variable"",
                ""property"": ""hydra:property"",
                ""required"": ""hydra:required"",
                ""readonly"": ""hydra:readonly"",
                ""writeonly"": ""hydra:writeonly"",
                ""method"": ""hydra:method"",
                ""expects"": ""hydra:expects"",
                ""returns"": ""hydra:returns""
            }}",
            Rdfs.BaseUri,
            Hydra);

        private IEntityContextFactory _entityContextFactory;

        /// <summary>Initializes a new instance of the <see cref="EntityConverter" /> class.</summary>
        /// <param name="entityContextFactory">Entity context factory.</param>
        public EntityConverter(IEntityContextFactory entityContextFactory)
        {
            if (entityContextFactory == null)
            {
                throw new ArgumentNullException("entityContextFactory");
            }

            _entityContextFactory = entityContextFactory;
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CanConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentOutOfRangeException("request");
            }

            if (!typeof(IEntity).IsAssignableFrom(expectedType))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.TypeMatch;
            var requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            if ((contentType != null) && (contentType.Values.Join(SupportedMediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer).Any()))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            return (T)typeof(EntityExtensions).GetMethod("AsEntity", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(typeof(T))
                .Invoke(null, new object[] { (IEntity)ConvertTo(typeof(T), request) });
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (!typeof(IEntity).IsAssignableFrom(expectedType))
            {
                throw new ArgumentOutOfRangeException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentOutOfRangeException("request");
            }

            var requestInfo = (RequestInfo)request;
            var accept = requestInfo.Headers[Header.Accept];
            var mediaType = accept.Values.Join(SupportedMediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer).First();
            ITripleStore store = new TripleStore();
            IGraph graph = new Graph();
            store.Add(graph);
            var reader = CreateReader(mediaType.Value);
            using (var textReader = new StreamReader(requestInfo.Body))
            {
                reader.Load(graph, textReader);
            }

            ((IComponentRegistryFacade)_entityContextFactory).Register(store);
            var context = _entityContextFactory.CreateContext();
            var entity = (IEntity)context.GetType().GetInterfaceMap(typeof(IEntityContext))
                .TargetMethods
                .Where(method => method.Name == "Create")
                .First()
                .MakeGenericMethod(expectedType)
                .Invoke(context, new object[] { new EntityId(request.Uri) });
            return entity;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(string body)
        {
            return (T)ConvertTo(typeof(T), body);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, string body)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (!typeof(IEntity).IsAssignableFrom(givenType))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.TypeMatch;
            var requestInfo = (RequestInfo)response.Request;
            var accept = requestInfo.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Join(SupportedMediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer).Any()))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public void ConvertFrom<T>(T instance, IResponseInfo response)
        {
            ConvertFrom(typeof(T), instance, response);
        }

        /// <inheritdoc />
        public void ConvertFrom(Type givenType, object instance, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (instance != null)
            {
                if (!givenType.IsAssignableFrom(instance.GetType()))
                {
                    throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
                }

                var requestInfo = (RequestInfo)response.Request;
                var accept = requestInfo.Headers[Header.Accept];
                var mediaType = accept.Values.Join(SupportedMediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer.Value).First();
                var entity = (IEntity)instance;
                ITripleStore store = new TripleStore();
                IGraph graph = new Graph();
                graph.Assert(entity.Context.Store.Quads.Select(quad =>
                    new Triple(quad.Subject.UnWrapNode(graph), quad.Predicate.UnWrapNode(graph), quad.Object.UnWrapNode(graph))));
                var writer = CreateWriter(mediaType);
                if (writer is RdfXmlWriter)
                {
                    Stream buffer = new MemoryStream();
                    buffer = new UnclosableStream(buffer);
                    using (var textWriter = new StreamWriter(buffer))
                    {
                        writer.Save(graph, textWriter);
                    }

                    buffer.Seek(0, SeekOrigin.Begin);
                    XmlDocument document = new XmlDocument();
                    document.Load(buffer);
                    document.InsertAfter(
                        document.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + DocumentationStylesheet + "\""),
                        document.FirstChild);
                    document.Save(response.Body);
                }
                else
                {
                    using (var textWriter = new StreamWriter(response.Body))
                    {
                        writer.Save(graph, textWriter);
                    }
                }
            }
        }

        private IRdfReader CreateReader(string mediaType)
        {
            IRdfReader result = null;
            switch (mediaType)
            {
                case TextTurtle:
                    result = new TurtleParser();
                    break;
                case ApplicationRdfXml:
                case ApplicationOwlXml:
                    result = new RdfXmlParser();
                    break;
                case ApplicationLdJson:
                    result = new JsonLdParser();
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Media type '{0}' is not supported.", mediaType));
            }

            return result;
        }

        private IRdfWriter CreateWriter(string mediaType)
        {
            IRdfWriter result = null;
            switch (mediaType)
            {
                case TextTurtle:
                    result = new CompressingTurtleWriter();
                    break;
                case ApplicationRdfXml:
                case ApplicationOwlXml:
                    result = new RdfXmlWriter();
                    break;
                case ApplicationLdJson:
                    result = new JsonLdWriter(Context);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Media type '{0}' is not supported.", mediaType));
            }

            return result;
        }
    }
}