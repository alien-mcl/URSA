using RomanticWeb;
using RomanticWeb.ComponentModel;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using URSA.Web.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.NamedGraphs;
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
        public static readonly string[] MediaTypes = new[] { TextTurtle, ApplicationRdfXml, ApplicationOwlXml, ApplicationLdJson };

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
        private readonly INamedGraphSelectorFactory _namedGraphSelectorFactory;

        static EntityConverter()
        {
            MediaTypeFileFormats[TextTurtle] = "ttl";
            MediaTypeFileFormats[ApplicationRdfXml] = "rdf";
            MediaTypeFileFormats[ApplicationOwlXml] = "owl";
            MediaTypeFileFormats[ApplicationLdJson] = "jsonld";
        }

        /// <summary>Initializes a new instance of the <see cref="EntityConverter" /> class.</summary>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="namedGraphSelectorFactory">Named graph selector factory.</param>
        public EntityConverter(IEntityContextProvider entityContextProvider, INamedGraphSelectorFactory namedGraphSelectorFactory)
        {
            if (entityContextProvider == null)
            {
                throw new ArgumentNullException("entityContextProvider");
            }

            if (namedGraphSelectorFactory == null)
            {
                throw new ArgumentNullException("namedGraphSelectorFactory");
            }

            _entityContextProvider = entityContextProvider;
            _namedGraphSelectorFactory = namedGraphSelectorFactory;
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
            if (!typeof(IEntity).IsAssignableFrom(actualExpectedType))
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
                throw new ArgumentNullException("request");
            }

            var requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            var mediaType = (contentType != null ? contentType.Values.Join(MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => outer.Value).First() : TextTurtle);
            var reader = CreateReader(mediaType);
            var entityId = request.Uri.GetLeftPart(UriPartial.Path).TrimEnd('/') + '/' + request.Uri.Query;
            var graphUri = _namedGraphSelectorFactory.NamedGraphSelector.SelectGraph(new EntityId(entityId), null, null);
            var graph = _entityContextProvider.TripleStore.FindOrCreate(graphUri);
            graph.Clear();
            using (var textReader = new StreamReader(requestInfo.Body))
            {
                reader.Load(graph, textReader);
            }

            _entityContextProvider.TripleStore.MapToMetaGraph(graph.BaseUri);
            var entity = (IEntity)_entityContextProvider.EntityContext.GetType().GetInterfaceMap(typeof(IEntityContext))
                .TargetMethods
                .First(method => method.Name == "Load")
                .MakeGenericMethod(expectedType)
                .Invoke(_entityContextProvider.EntityContext, new object[] { new EntityId(entityId) });
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

            var actualGivenType = givenType.FindItemType();
            if (!typeof(IEntity).IsAssignableFrom(actualGivenType))
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
            var graph = CreateResultingGraph(instance, requestInfo.Uri);
            WriteResponseBody(graph, mediaType, response);
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

        private IGraph CreateResultingGraph(object instance, Uri requestUri)
        {
            var entities = (instance is IEnumerable<IEntity> ? (IEnumerable<IEntity>)instance : new[] { (IEntity)instance });
            IGraph graph = new Graph();
            var relatedEntities = new List<IUriNode>();
            var visitedEntities = new List<string>();
            foreach (var entity in entities)
            {
                AssertEntityTriples(entity, graph, relatedEntities, visitedEntities);
            }

            for (int index = 0; index < relatedEntities.Count; index++)
            {
                AssertEntityTriples(index, graph, relatedEntities, visitedEntities);
            }

            if (!visitedEntities.Contains(requestUri.ToString()))
            {
                relatedEntities.Add(_entityContextProvider.TripleStore.Graphs.First().CreateUriNode(requestUri));
                int startAt = relatedEntities.Count - 1;
                AssertEntityTriples(startAt, graph, relatedEntities, visitedEntities);
                for (int index = startAt + 1; index < relatedEntities.Count; index++)
                {
                    AssertEntityTriples(index, graph, relatedEntities, visitedEntities);
                }
            }

            return graph;
        }

        private void AssertEntityTriples(IEntity entity, IGraph graph, IList<IUriNode> relatedEntities, IList<string> visitedEntities)
        {
            var graphUri = _namedGraphSelectorFactory.NamedGraphSelector.SelectGraph(entity.Id, null, null);
            foreach (var triple in _entityContextProvider.TripleStore.Graphs[graphUri].Triples)
            {
                if (triple.Subject is IUriNode)
                {
                    visitedEntities.Add(((IUriNode)triple.Subject).Uri.ToString());
                }

                if (triple.Object is IUriNode)
                {
                    var uriNode = (IUriNode)triple.Object;
                    var uriNodeGraphUri = _namedGraphSelectorFactory.NamedGraphSelector.SelectGraph(new EntityId(uriNode.Uri), null, null);
                    if ((!AbsoluteUriComparer.Default.Equals(uriNodeGraphUri, graphUri)) && (!relatedEntities.Contains(uriNode)))
                    {
                        relatedEntities.Add(uriNode);
                    }
                }

                graph.Assert(triple.CopyTriple(graph));
            }
        }

        private void AssertEntityTriples(int index, IGraph graph, IList<IUriNode> relatedEntities, IList<string> visitedEntities)
        {
            var entity = relatedEntities[index];
            var graphUri = _namedGraphSelectorFactory.NamedGraphSelector.SelectGraph(new EntityId(entity.Uri), null, null);
            var sourceGraph = _entityContextProvider.TripleStore.Graphs.FirstOrDefault(existingGraph => AbsoluteUriComparer.Default.Equals(existingGraph.BaseUri, graphUri));
            if (sourceGraph == null)
            {
                return;
            }

            foreach (var triple in sourceGraph.Triples)
            {
                if (triple.Subject is IUriNode)
                {
                    visitedEntities.Add(((IUriNode)triple.Subject).Uri.ToString());
                }

                if (triple.Object is IUriNode)
                {
                    var uriNode = (IUriNode)triple.Object;
                    var uriNodeGraphUri = _namedGraphSelectorFactory.NamedGraphSelector.SelectGraph(new EntityId(uriNode.Uri), null, null);
                    if ((!AbsoluteUriComparer.Default.Equals(uriNodeGraphUri, graphUri)) && (!relatedEntities.Contains(uriNode)))
                    {
                        relatedEntities.Add(uriNode);
                    }
                }

                graph.Assert(triple.CopyTriple(graph));
            }
        }

        private void WriteResponseBody(IGraph graph, string mediaType, IResponseInfo response)
        {
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