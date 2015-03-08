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

        /// <summary>XML format.</summary>
        Xml
    }

    /// <summary>Provides a ReST description facility.</summary>
    /// <typeparam name="T">Type of the controller to generate documentation for.</typeparam>
    [DependentRoute]
    public class DescriptionController<T> : IController where T : IController
    {
        private static ApiDescriptionBuilder<T> apiDescriptionBuilder;

        private IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="DescriptionController{T}" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="descriptionBuilder">Controller description builder.</param>
        public DescriptionController(IEntityContext entityContext, IHttpControllerDescriptionBuilder<T> descriptionBuilder)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            if ((apiDescriptionBuilder == null) && (descriptionBuilder == null))
            {
                throw new ArgumentNullException("descriptionBuilder");
            }

            _entityContext = entityContext;
            if (apiDescriptionBuilder == null)
            {
                apiDescriptionBuilder = new ApiDescriptionBuilder<T>(descriptionBuilder);
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
            if ((accept != null) && (accept.Contains("*/*")))
            {
                switch ((OutputFormats)format)
                {
                    case OutputFormats.Turtle:
                        Response.Request.Headers[Header.Accept] = EntityConverter.TextTurtle;
                        break;
                    case OutputFormats.Xml:
                        Response.Request.Headers[Header.Accept] = EntityConverter.ApplicationRdfXml;
                        break;
                }
            }

            IApiDocumentation result = _entityContext.Create<IApiDocumentation>(new EntityId(Response.Request.Uri.AddFragment(String.Empty)));
            apiDescriptionBuilder.BuildDescription(result);
            return result;
        }
    }
}