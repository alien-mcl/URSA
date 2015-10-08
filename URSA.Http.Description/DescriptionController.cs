using RomanticWeb;
using RomanticWeb.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

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
        public static readonly Uri VocabularyBaseUri = new Uri("http://github.io/ursa/vocabulary#");

        /// <summary>Defines a '<![CDATA[application/xml]]>' media type.</summary>
        private const string ApplicationXml = "application/xml";

        private readonly IApiDescriptionBuilder _apiDescriptionBuilder;
        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="DescriptionController" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        protected DescriptionController(IEntityContext entityContext, IApiDescriptionBuilder apiDescriptionBuilder)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            if (apiDescriptionBuilder == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilder");
            }

            _entityContext = entityContext;
            _apiDescriptionBuilder = apiDescriptionBuilder;
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
        [OnOptions]
        [OnGet]
        public IApiDocumentation GetApiEntryPointDescription(OutputFormats? format = null)
        {
            var fileExtension = OverrideAcceptedMediaType(format);
            ((ResponseInfo)Response).Headers.ContentDisposition = String.Format("inline; filename=\"{0}.{1}\"", FileName, fileExtension);
            IApiDocumentation result = _entityContext.Create<IApiDocumentation>(new EntityId(Response.Request.Uri.AddFragment(String.Empty)));
            _apiDescriptionBuilder.BuildDescription(result, RequestedMediaTypeProfiles);
            return result;
        }

        //// TODO: Check the default file name is actually a TXT!
        private string OverrideAcceptedMediaType(OutputFormats? format)
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

            var accept = Response.Request.Headers[Header.Accept];
            var fileExtension = "txt";
            if ((accept == null) || (!accept.Contains("*/*")))
            {
                return fileExtension;
            }

            var resultingMediaType = ((RequestInfo)Response.Request).Headers[Header.Accept].Values
                .Join(EntityConverter.MediaTypes, outer => outer.Value, inner => inner, (outer, inner) => inner)
                .FirstOrDefault();
            return (EntityConverter.MediaTypeFileFormats.ContainsKey(resultingMediaType) ? EntityConverter.MediaTypeFileFormats[resultingMediaType] : "txt");
        }
    }

    /// <summary>Provides a ReST description facility.</summary>
    /// <typeparam name="T">Type of the controller to generate documentation for.</typeparam>
    [DependentRoute]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppression is OK - generic and non-generic class.")]
    public class DescriptionController<T> : DescriptionController where T : IController
    {
        /// <summary>Initializes a new instance of the <see cref="DescriptionController{T}" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        public DescriptionController(IEntityContext entityContext, IApiDescriptionBuilder<T> apiDescriptionBuilder) : base(entityContext, apiDescriptionBuilder)
        {
        }

        /// <inheritdoc />
        protected internal override string FileName { get { return typeof(T).Name; } }
    }
}