﻿using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using RomanticWeb.NamedGraphs;
using URSA.Web.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.VDS.RDF;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using EntityExtensions = RomanticWeb.Entities.EntityExtensions;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts entities from and to RDF serialization.</summary>
    public class EntityConverter : IConverter
    {
        /// <summary>Defines a document name for <![CDATA[XML/XSLT]]> documentation style-sheet.</summary>
        public const string DocumentationStylesheet = "documentation-stylesheet";

        /// <summary>Defines a document name for documentation's property icon.</summary>
        public const string PropertyIcon = "property";

        /// <summary>Defines a document name for documentation's method icon.</summary>
        public const string MethodIcon = "method";

        /// <summary>Defines a '<![CDATA[text/turtle]]>' media type.</summary>
        public const string TextTurtle = "text/turtle";

        /// <summary>Defines a '<![CDATA[application/rdf+xml]]>' media type.</summary>
        public const string ApplicationRdfXml = "application/rdf+xml";

        /// <summary>Defines a '<![CDATA[application/owl+xml]]>' media type.</summary>
        public const string ApplicationOwlXml = "application/owl+xml";

        /// <summary>Defines a '<![CDATA[application/ld+json]]>' media type.</summary>
        public const string ApplicationLdJson = "application/ld+json";

        /// <summary>Defines the supported media types.</summary>
        public static readonly string[] MediaTypes = { TextTurtle, ApplicationRdfXml, ApplicationOwlXml, ApplicationLdJson };

        /// <summary>Gets the media type file format mapping.</summary>
        public static readonly IDictionary<string, string> MediaTypeFileFormats = new ConcurrentDictionary<string, string>();

        /// <summary>Defines a <![CDATA[HYpermedia DRiven Application (HYDRA)]]> vocabulary Uri.</summary>
        public static readonly Uri Hydra = new Uri("http://www.w3.org/ns/hydra/core#");

        /// <summary>Defines a <![CDATA[SHApes Constraints Language (SHACL)]]> vocabulary Uri.</summary>
        public static readonly Uri Shacl = new Uri("http://www.w3.org/ns/shacl#");

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
                ""supportedProperty"": ""hydra:supportedProperty"",
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

        private readonly IEntityContextProvider _entityContextProvider;
        private readonly INamedGraphSelector _namedGraphSelector;

        static EntityConverter()
        {
            MediaTypeFileFormats[TextTurtle] = "ttl";
            MediaTypeFileFormats[ApplicationRdfXml] = "rdf";
            MediaTypeFileFormats[ApplicationOwlXml] = "owl";
            MediaTypeFileFormats[ApplicationLdJson] = "jsonld";
        }

        /// <summary>Initializes a new instance of the <see cref="EntityConverter" /> class.</summary>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="namedGraphSelector">Named graph selector.</param>
        public EntityConverter(IEntityContextProvider entityContextProvider, INamedGraphSelector namedGraphSelector)
        {
            if (entityContextProvider == null)
            {
                throw new ArgumentNullException("entityContextProvider");
            }

            if (namedGraphSelector == null)
            {
                throw new ArgumentNullException("namedGraphSelector");
            }

            _entityContextProvider = entityContextProvider;
            _namedGraphSelector = namedGraphSelector;
        }

        /// <inheritdoc />
        public IEnumerable<string> SupportedMediaTypes { get { return MediaTypes; } } 

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
                throw new ArgumentNullException("request");
            }

            var actualExpectedType = expectedType.FindItemType();
            if (!typeof(IEntity).GetTypeInfo().IsAssignableFrom(actualExpectedType))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            if ((contentType != null) && (contentType.Values.Join(MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer).Any()))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            return (T)ConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            var itemType = expectedType.FindItemType();
            if (!typeof(IEntity).GetTypeInfo().IsAssignableFrom(itemType))
            {
                throw new ArgumentOutOfRangeException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            var mediaType = (contentType != null ? contentType.Values.Join(MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer.Value).First() : TextTurtle);
            var reader = CreateReader(mediaType);
            var entityId = (Uri)requestInfo.Url.WithFragment(null);
            var graphUri = _namedGraphSelector.SelectGraph(new EntityId(entityId), null, null);
            var graph = _entityContextProvider.TripleStore.FindOrCreate(graphUri);
            graph.Clear();
            using (var textReader = new StreamReader(requestInfo.Body))
            {
                reader.Load(graph, textReader);
            }

            _entityContextProvider.TripleStore.MapToMetaGraph(graph.BaseUri);
            if (itemType == expectedType)
            {
                return _entityContextProvider.EntityContext.GetType().GetTypeInfo().GetRuntimeInterfaceMap(typeof(IEntityContext))
                    .TargetMethods
                    .First(method => method.Name == "Load")
                    .MakeGenericMethod(itemType)
                    .Invoke(_entityContextProvider.EntityContext, new object[] { new EntityId(entityId) });
            }

            var result = _entityContextProvider.EntityContext.GetType().GetTypeInfo().GetRuntimeInterfaceMap(typeof(IEntityContext))
                .TargetMethods
                .First(method => (method.Name == "AsQueryable") && (method.IsGenericMethod))
                .MakeGenericMethod(itemType)
                .Invoke(_entityContextProvider.EntityContext, null);
            result = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(itemType).Invoke(null, new[] { result });
            return (expectedType.IsArray ? result.GetType().GetMethod("ToArray").Invoke(result, null) : result);
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

            var actualGivenType = givenType.FindItemType();
            if (!typeof(IEntity).GetTypeInfo().IsAssignableFrom(actualGivenType))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var requestInfo = (RequestInfo)response.Request;
            var accept = requestInfo.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Join(MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer).Any()))
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

            if (instance == null)
            {
                return;
            }

            if (!givenType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
            }

            var responseInfo = (ResponseInfo)response;
            var requestInfo = responseInfo.Request;
            var accept = requestInfo.Headers[Header.Accept];
            var mediaType = accept.Values.Join(MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer.Value).First();
            if (String.IsNullOrEmpty(responseInfo.Headers.ContentType))
            {
                responseInfo.Headers.ContentType = mediaType;
            }

            //// TODO: Add support for graph based serializations.
            var graphs = _entityContextProvider.TripleStore.Graphs.Where(graph => !AbsoluteUriComparer.Default.Equals(graph.BaseUri, _entityContextProvider.MetaGraph));
            if (graphs.Any())
            {
                WriteResponseBody(graphs, mediaType, response);
            }
        }

        private static IRdfReader CreateReader(string mediaType)
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

        private static IRdfWriter CreateWriter(string mediaType)
        {
            IRdfWriter result;
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

            if (!(result is INamespaceWriter))
            {
                return result;
            }

            var namespaceWriter = (INamespaceWriter)result;
            namespaceWriter.DefaultNamespaces.AddNamespace("owl", new Uri(Owl.BaseUri));
            namespaceWriter.DefaultNamespaces.AddNamespace("hydra", Hydra);
            namespaceWriter.DefaultNamespaces.AddNamespace("ursa", DescriptionController<IController>.VocabularyBaseUri);
            return result;
        }

        private void WriteResponseBody(IEnumerable<IGraph> graphs, string mediaType, IResponseInfo response)
        {
            var graph = new UnionGraph(graphs.First(), graphs.Skip(1));
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
}