using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities;
using URSA.Reflection;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description
{
    /// <summary>Generates an API entry point description.</summary>
    public class EntryPointDescriptionController : DescriptionController
    {
        private readonly IEnumerable<IHttpControllerDescriptionBuilder> _httpControllerDescriptionBuilders;
        private readonly IApiEntryPointDescriptionBuilder _apiDescriptionBuilder;

        /// <summary>Initializes a new instance of the <see cref="EntryPointDescriptionController"/> class.</summary>
        /// <param name="entryPoint">Entry point URL.</param>
        /// <param name="entityContextProvider">Entity context provider.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        /// <param name="httpControllerDescriptionBuilders">HTTP Controller description builders.</param>
        [ExcludeFromCodeCoverage]
        public EntryPointDescriptionController(
            Url entryPoint,
            IEntityContextProvider entityContextProvider,
            IApiEntryPointDescriptionBuilder apiDescriptionBuilder,
            IEnumerable<IHttpControllerDescriptionBuilder> httpControllerDescriptionBuilders) :
            base(entityContextProvider, apiDescriptionBuilder)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }

            _httpControllerDescriptionBuilders = httpControllerDescriptionBuilders;
            (_apiDescriptionBuilder = apiDescriptionBuilder).EntryPoint = entryPoint;
        }

        /// <summary>Gets the entry point.</summary>
        [ExcludeFromCodeCoverage]
        public Url EntryPoint { get { return _apiDescriptionBuilder.EntryPoint; } }

        /// <inheritdoc />
        protected internal override string FileName
        {
            get
            {
                var entryPoint = EntryPoint.ToString();
                int position = entryPoint.IndexOf('#');
                return (position != -1 ? entryPoint.Substring(position + 1) : entryPoint.Split('/').Last());
            }
        }
        
        /// <summary>Gets the hypermedia enabled controller list operations.</summary>
        /// <returns>Returns entry point details.</returns>
        [OnGet]
        [Route("/")]
        public IEntity GetEntryPoint()
        {
            foreach (var controllerDescriptionBuilder in _httpControllerDescriptionBuilders)
            {
                var controllerInfo = controllerDescriptionBuilder.BuildDescriptor();
                if (controllerInfo.ControllerType.GetInterfaceImplementation(typeof(IHypermediaDrivenController<,>)) == null)
                {
                    continue;
                }

                foreach (var @interface in controllerInfo.ControllerType.GetTypeInfo().GetInterfaces()
                    .Where(implemented => (implemented.GetTypeInfo().IsGenericType) && (implemented.GetGenericTypeDefinition() == typeof(IController<>))))
                {
                    controllerInfo.ControllerType.InjectOperation(
                        controllerInfo.ControllerType.GetTypeInfo().GetRuntimeInterfaceMap(@interface).TargetMethods.First(method => method.Name == "List"),
                        Response);
                }
            }

            return EntityContextProvider.EntityContext.Load<IEntity>(new EntityId((Uri)Response.Request.Url));
        }
    }
}