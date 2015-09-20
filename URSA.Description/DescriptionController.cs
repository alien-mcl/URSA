using RomanticWeb;
using RomanticWeb.Entities;
using System;
using System.Collections.Generic;
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

        /// <summary>XML format.</summary>
        Xml
    }

    /// <summary>Provides a ReST description facility.</summary>
    /// <typeparam name="T">Type of the controller to generate documentation for.</typeparam>
    [DependentRoute]
    public class DescriptionController<T> : IController where T : IController
    {
        /// <summary>Defines an URSA vocabulary base URI.</summary>
        public static readonly Uri VocabularyBaseUri = new Uri("http://github.io/ursa/vocabulary#");

        /// <summary>Defines a '<![CDATA[application/xml]]>' media type.</summary>
        private const string ApplicationXml = "application/xml";

        private readonly IApiDescriptionBuilder<T> _apiDescriptionBuilder;
        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="DescriptionController{T}" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        public DescriptionController(IEntityContext entityContext, IApiDescriptionBuilder<T> apiDescriptionBuilder)
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
                return (from value in request.Headers[Header.Link].Values from parameter in value.Parameters where parameter.Name == "rel" select new Uri(value.Value))
                    .Union(from value in request.Headers[Header.Accept].Values from parameter in value.Parameters where parameter.Name == "profile" select (Uri)parameter.Value);
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
            var fileExtension = OverrideAcceptedMediaType(format ?? OutputFormats.Turtle);
            ((ResponseInfo)Response).Headers.ContentDisposition = String.Format("inline; filename=\"{0}.{1}\"", typeof(T).Name, fileExtension);
            IApiDocumentation result = _entityContext.Create<IApiDocumentation>(new EntityId(Response.Request.Uri.AddFragment(String.Empty)));
            _apiDescriptionBuilder.BuildDescription(result, RequestedMediaTypeProfiles);
            return result;
        }

        //// TODO: Check the default file name is actually a TXT!
        private string OverrideAcceptedMediaType(OutputFormats? format)
        {
            var accept = Response.Request.Headers[Header.Accept];
            var fileExtension = "txt";
            if ((accept == null) || (!accept.Contains("*/*")))
            {
                return fileExtension;
            }

            switch ((OutputFormats)format)
            {
                case OutputFormats.Turtle:
                    Response.Request.Headers[Header.Accept] = EntityConverter.TextTurtle;
                    fileExtension = "ttl";
                    break;
                case OutputFormats.Xml:
                    Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationRdfXml;
                    Response.Headers[Header.ContentType] = ApplicationXml;
                    fileExtension = "xml";
                    break;
                case OutputFormats.Rdf:
                    Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationRdfXml;
                    fileExtension = "rdf";
                    break;
            }

            return fileExtension;
        }
    }
}