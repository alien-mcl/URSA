using RomanticWeb;
using RomanticWeb.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RomanticWeb.NamedGraphs;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using VDS.RDF;

namespace URSA.Web.Http.Description
{
    /// <summary>Defines possible documentation output formats.</summary>
    public enum OutputFormats
    {
        /// <summary>Turtle format.</summary>
        Turtle,
        
        /// <summary>RDF/XML format.</summary>
        Rdf,

        /// <summary>OWL/XML format.</summary>
        Owl,

        /// <summary>XML alias for RDF/XML format.</summary>
        Xml,

        /// <summary>JSON-LD format.</summary>
        JsonLd
    }

    /// <summary>Describes an abstract description controller.</summary>
    public abstract class DescriptionController : IController
    {
        /// <summary>Defines an URSA vocabulary base URI.</summary>
        public static readonly Uri VocabularyBaseUri = new Uri("http://alien-mcl.github.io/URSA/vocabulary#");

        /// <summary>Defines a '<![CDATA[application/xml]]>' media type.</summary>
        private const string ApplicationXml = "application/xml";

        private readonly IEntityContextProvider _entityContextProvider;
        private readonly IApiDescriptionBuilder _apiDescriptionBuilder;
        private readonly INamedGraphSelectorFactory _namedGraphSelectorFactory;

        /// <summary>Initializes a new instance of the <see cref="DescriptionController" /> class.</summary>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        /// <param name="namedGraphSelectorFactory">Named graph selector factory.</param>
        protected DescriptionController(IEntityContextProvider entityContextProvider, IApiDescriptionBuilder apiDescriptionBuilder, INamedGraphSelectorFactory namedGraphSelectorFactory)
        {
            if (entityContextProvider == null)
            {
                throw new ArgumentNullException("entityContextProvider");
            }

            if (apiDescriptionBuilder == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilder");
            }

            if (namedGraphSelectorFactory == null)
            {
                throw new ArgumentNullException("namedGraphSelectorFactory");
            }

            _entityContextProvider = entityContextProvider;
            _apiDescriptionBuilder = apiDescriptionBuilder;
            _namedGraphSelectorFactory = namedGraphSelectorFactory;
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets the name of the file.</summary>
        protected internal abstract string FileName { get; }

        /// <summary>Gets the API description builder.</summary>
        protected IApiDescriptionBuilder ApiDescriptionBuilder { get { return _apiDescriptionBuilder; } }

        private IEnumerable<Uri> RequestedMediaTypeProfiles
        {
            get
            {
                if (!(Response.Request is RequestInfo))
                {
                    return new Uri[0];
                }

                //// TODO: Introduce strongly typed header/parameter parsing routines.
                RequestInfo request = (RequestInfo)Response.Request;
                IEnumerable<Uri> result = null;
                var accept = request.Headers[Header.Accept];
                if (accept != null)
                {
                    result = from value in accept.Values from parameter in value.Parameters where parameter.Name == "profile" select (Uri)parameter.Value;
                }

                var link = request.Headers[Header.Link];
                if (link == null)
                {
                    return result;
                }

                var linkProfile = from value in link.Values from parameter in value.Parameters where parameter.Name == "rel" select new Uri(value.Value);
                return (result == null ? linkProfile : result.Union(linkProfile));
            }
        }

        /// <summary>Gets the API documentation.</summary>
        /// <param name="format">Optional output format.</param>
        /// <returns><see cref="IApiDocumentation" /> instance.</returns>
        [Route("/")]
        [OnGet]
        public IApiDocumentation GetApiEntryPointDescription(OutputFormats format)
        {
            return GetApiEntryPointDescription(OverrideAcceptedMediaType(format));
        }

        /// <summary>Gets the API documentation.</summary>
        /// <returns><see cref="IApiDocumentation" /> instance.</returns>
        [Route("/")]
        [OnOptions]
        public IApiDocumentation GetApiEntryPointDescription()
        {
            return GetApiEntryPointDescription(OverrideAcceptedMediaType(null));
        }

        private IApiDocumentation GetApiEntryPointDescription(string fileExtension)
        {
            var namedGraphSelector = _namedGraphSelectorFactory.NamedGraphSelector;
            ILocallyControlledNamedGraphSelector locallyControlledNamedGraphSelector = namedGraphSelector as ILocallyControlledNamedGraphSelector;
            return locallyControlledNamedGraphSelector != null ?
                GetApiEntryPointDescriptionWithLock(fileExtension, locallyControlledNamedGraphSelector) :
                BuildApiDocumentation(fileExtension, (Uri)((HttpUrl)Response.Request.Url).WithFragment(String.Empty));
        }

        private IApiDocumentation GetApiEntryPointDescriptionWithLock(string fileExtension, ILocallyControlledNamedGraphSelector locallyControlledNamedGraphSelector)
        {
            IApiDocumentation result;
            lock (locallyControlledNamedGraphSelector)
            {
                Uri graphUri;
                HttpUrl url = Response.Request.Url as HttpUrl;
                if (url != null)
                {
                    if (url.HasParameters)
                    {
                        url = (HttpUrl)url.WithoutParameters();
                    }

                    graphUri = (Uri)url.WithFragment(String.Empty);
                }
                else
                {
                    graphUri = (Uri)Response.Request.Url;
                }

                result = BuildApiDocumentation(fileExtension, locallyControlledNamedGraphSelector.NamedGraph = graphUri);
                _entityContextProvider.EntityContext.Disposed += () => _entityContextProvider.TripleStore.Remove(graphUri);
                locallyControlledNamedGraphSelector.NamedGraph = null;
            }

            return result;
        }

        private IApiDocumentation BuildApiDocumentation(string fileExtension, Uri entityId)
        {
            ((ResponseInfo)Response).Headers.ContentDisposition = String.Format("inline; filename=\"{0}.{1}\"", FileName, fileExtension);
            IApiDocumentation result = _entityContextProvider.EntityContext.Create<IApiDocumentation>(new EntityId(entityId));
            _apiDescriptionBuilder.BuildDescription(result, RequestedMediaTypeProfiles);
            _entityContextProvider.EntityContext.Commit();
            return result;
        }

        //// TODO: Check the default file name is actually a TXT!
        private string OverrideAcceptedMediaType(OutputFormats? format)
        {
            if (format != null)
            {
                switch ((OutputFormats)format)
                {
                    case OutputFormats.JsonLd:
                        return EntityConverter.MediaTypeFileFormats[Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationLdJson];
                    case OutputFormats.Turtle:
                        return EntityConverter.MediaTypeFileFormats[Response.Request.Headers[Header.Accept] = EntityConverter.TextTurtle];
                    case OutputFormats.Xml:
                        Response.Headers[Header.ContentType] = ApplicationXml;
                        return EntityConverter.MediaTypeFileFormats[Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationRdfXml];
                    case OutputFormats.Rdf:
                        return EntityConverter.MediaTypeFileFormats[Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationRdfXml];
                    case OutputFormats.Owl:
                        return EntityConverter.MediaTypeFileFormats[Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationRdfXml];
                }
            }

            var accept = Response.Request.Headers[Header.Accept];
            var fileExtension = "txt";
            if ((accept == null) || (!accept.Contains("*/*")))
            {
                return fileExtension;
            }

            var resultingMediaType = ((RequestInfo)Response.Request).Headers[Header.Accept].Values
                .Join(EntityConverter.MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => inner)
                .FirstOrDefault();
            return ((resultingMediaType == null) || (!EntityConverter.MediaTypeFileFormats.ContainsKey(resultingMediaType)) ? "txt" : EntityConverter.MediaTypeFileFormats[resultingMediaType]);
        }
    }

    /// <summary>Provides a ReST description facility.</summary>
    /// <typeparam name="T">Type of the controller to generate documentation for.</typeparam>
    [DependentRoute]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppression is OK - generic and non-generic class.")]
    public class DescriptionController<T> : DescriptionController where T : IController
    {
        /// <summary>Initializes a new instance of the <see cref="DescriptionController{T}" /> class.</summary>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        /// <param name="namedGraphSelectorFactory">Named graph selector factory.</param>
        public DescriptionController(IEntityContextProvider entityContextProvider, IApiDescriptionBuilder<T> apiDescriptionBuilder, INamedGraphSelectorFactory namedGraphSelectorFactory) :
            base(entityContextProvider, apiDescriptionBuilder, namedGraphSelectorFactory)
        {
        }

        /// <inheritdoc />
        protected internal override string FileName { get { return typeof(T).Name; } }
    }
}