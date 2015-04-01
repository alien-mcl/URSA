using RomanticWeb;
using RomanticWeb.Entities;
using System;
using URSA.Web.Description.Http;
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
        /// <summary>Defines a '<![CDATA[application/xml]]>' media type.</summary>
        private const string ApplicationXml = "application/xml";

        private static ApiDescriptionBuilder<T> _apiDescriptionBuilder;

        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="DescriptionController{T}" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="descriptionBuilder">Controller description builder.</param>
        /// <param name="xmlDocProvider">The XML documentation provider.</param>
        public DescriptionController(IEntityContext entityContext, IHttpControllerDescriptionBuilder<T> descriptionBuilder, IXmlDocProvider xmlDocProvider)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            if ((_apiDescriptionBuilder == null) && (descriptionBuilder == null))
            {
                throw new ArgumentNullException("descriptionBuilder");
            }

            _entityContext = entityContext;
            if (_apiDescriptionBuilder == null)
            {
                _apiDescriptionBuilder = new ApiDescriptionBuilder<T>(descriptionBuilder, xmlDocProvider);
            }
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets the API documentation.</summary>
        /// <param name="format">Optional output format.</param>
        /// <returns><see cref="IApiDocumentation" /> instance.</returns>
        [Route("/")]
        [OnOptions]
        [OnGet]
        public IApiDocumentation GetApiEntryPointDescription(OutputFormats? format = null)
        {
            format = format ?? OutputFormats.Turtle;
            var accept = Response.Request.Headers[Header.Accept];
            var fileExtension = "txt";
            if ((accept != null) && (accept.Contains("*/*")))
            {
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
            }

            ((ResponseInfo)Response).Headers.ContentDisposition = String.Format("inline; filename=\"{0}.{1}\"", typeof(T).Name, fileExtension);
            IApiDocumentation result = _entityContext.Create<IApiDocumentation>(new EntityId(Response.Request.Uri.AddFragment(String.Empty)));
            _apiDescriptionBuilder.BuildDescription(result);
            return result;
        }
    }
}