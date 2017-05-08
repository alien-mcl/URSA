using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RDeF.Entities;
using URSA.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description
{
    /// <summary>Generates an API entry point description.</summary>
    public class EntryPointDescriptionController : DescriptionController
    {
        private readonly IHttpServerConfiguration _httpServerConfiguration;
        private readonly IEnumerable<IHttpControllerDescriptionBuilder> _httpControllerDescriptionBuilders;
        private readonly IApiEntryPointDescriptionBuilder _apiDescriptionBuilder;

        /// <summary>Initializes a new instance of the <see cref="EntryPointDescriptionController"/> class.</summary>
        /// <param name="entryPoint">Entry point URL.</param>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="apiDescriptionBuilder">API description builder.</param>
        /// <param name="httpServerConfiguration">HTTP server configuration with base Uri.</param>
        /// <param name="httpControllerDescriptionBuilders">HTTP Controller description builders.</param>
        [ExcludeFromCodeCoverage]
        public EntryPointDescriptionController(
            Url entryPoint,
            IEntityContext entityContext,
            IApiEntryPointDescriptionBuilder apiDescriptionBuilder,
            IHttpServerConfiguration httpServerConfiguration,
            IEnumerable<IHttpControllerDescriptionBuilder> httpControllerDescriptionBuilders) :
            base(entityContext, apiDescriptionBuilder)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }

            _httpServerConfiguration = httpServerConfiguration;
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
                    var methodInfo = controllerInfo.ControllerType.GetTypeInfo().GetRuntimeInterfaceMap(@interface).TargetMethods.First(method => method.Name == "List");
                    var hypermediaControls = new OperationHypermediaControl(
                        HypermediaControlRules.Include,
                        (OperationInfo<Verb>)controllerInfo.Operations.First(operation => operation.UnderlyingMethod == methodInfo),
                        _apiDescriptionBuilder,
                        EntityContext,
                        _httpServerConfiguration);
                    Response.Request.HypermediaControls.Add(hypermediaControls);
                }
            }

            return EntityContext.Load<IEntity>(new Iri((Uri)Response.Request.Url));
        }
    }
}