using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides an API entry point description.</summary>
    public class ApiEntryPointDescriptionBuilder : IApiEntryPointDescriptionBuilder
    {
        private readonly IApiDescriptionBuilderFactory _apiDescriptionBuilderFactory;
        private readonly IEnumerable<IHttpControllerDescriptionBuilder> _controllerDescriptionBuilders;

        /// <summary>Initializes a new instance of the <see cref="ApiEntryPointDescriptionBuilder"/> class.</summary>
        /// <param name="apiDescriptionBuilderFactory">The API description builder factory.</param>
        /// <param name="controllerDescriptionBuilders">The controller description builders.</param>
        [ExcludeFromCodeCoverage]
        public ApiEntryPointDescriptionBuilder(IApiDescriptionBuilderFactory apiDescriptionBuilderFactory, IEnumerable<IHttpControllerDescriptionBuilder> controllerDescriptionBuilders)
        {
            if (apiDescriptionBuilderFactory == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilderFactory");
            }

            if (controllerDescriptionBuilders == null)
            {
                throw new ArgumentNullException("controllerDescriptionBuilders");
            }

            if (!controllerDescriptionBuilders.Any())
            {
                throw new ArgumentOutOfRangeException("controllerDescriptionBuilders");
            }

            _apiDescriptionBuilderFactory = apiDescriptionBuilderFactory;
            _controllerDescriptionBuilders = controllerDescriptionBuilders;
        }

        /// <inheritdoc />
        public Uri EntryPoint { get; set; }

        /// <inheritdoc />
        public void BuildDescription(IApiDocumentation apiDocumentation, IEnumerable<Uri> profiles)
        {
            foreach (var controllerDescriptionBuilder in _controllerDescriptionBuilders)
            {
                var controllerDescriptionBuilderType = controllerDescriptionBuilder.GetType();
                Type targetImplementation = null;
                controllerDescriptionBuilderType.GetInterfaces().FirstOrDefault(@interface => (@interface.IsGenericType) &&
                    (@interface.GetGenericTypeDefinition() == typeof(IHttpControllerDescriptionBuilder<>)) &&
                    (typeof(IController).IsAssignableFrom(targetImplementation = @interface.GetGenericArguments()[0])));
                if ((targetImplementation == null) || ((targetImplementation.IsGenericType) && (targetImplementation.GetGenericTypeDefinition() == typeof(DescriptionController<>))))
                {
                    continue;
                }

                var controllerDescription = controllerDescriptionBuilder.BuildDescriptor();
                if ((controllerDescription.EntryPoint == null) || (controllerDescription.EntryPoint.ToString() != EntryPoint.ToString()))
                {
                    continue;
                }

                var apiDescriptionBuilder = _apiDescriptionBuilderFactory.Create(targetImplementation);
                apiDescriptionBuilder.BuildDescription(apiDocumentation, profiles);
            }
        }
    }
}