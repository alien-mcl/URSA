using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using RomanticWeb;
using RomanticWeb.DotNetRDF;
using System;
using System.Configuration;
using System.Reflection;
using System.Web;
using Castle.Facilities.TypedFactory;
using Castle.Facilities.TypedFactory.Internal;
using URSA.CastleWindsor.ComponentModel;
using URSA.CodeGen;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using VDS.RDF;

namespace URSA.CastleWindsor
{
    /// <summary>Installs HTTP components.</summary>
    public class HttpInstaller : IWindsorInstaller
    {
        private readonly Lazy<EntityContextFactory> _entityContextFactory;
        private readonly object _lock = new object();

        /// <summary>Initializes a new instance of the <see cref="HttpInstaller" /> class.</summary>
        public HttpInstaller()
        {
            _entityContextFactory = new Lazy<EntityContextFactory>(CreateEntityContextFactory);
        }

        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            WindsorComponentProvider componentProvider = container.Resolve<WindsorComponentProvider>();
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultValueRelationSelectorType != null) ?
                configuration.DefaultValueRelationSelectorType :
                typeof(DefaultValueRelationSelector));
            container.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For<IControllerActivator>().UsingFactoryMethod((kernel, context) => new DefaultControllerActivator(UrsaConfigurationSection.InitializeComponentProvider())).LifestyleSingleton());
            container.Register(Component.For<IEntityContext>().UsingFactoryMethod(CreateEntityContext).LifestylePerWebRequest());
            container.Register(Component.For<IEntityContextFactory>().Instance(_entityContextFactory.Value).LifestyleSingleton());
            container.Register(Component.For<IDefaultValueRelationSelector>().ImplementedBy(sourceSelectorType).LifestyleSingleton());
            container.Register(Component.For(typeof(DescriptionController<>)).Forward<IController>()
                .ImplementedBy(typeof(DescriptionController<>), componentProvider.GenericImplementationMatchingStrategy).LifestyleTransient());
            container.Register(Component.For<EntryPointDescriptionController>().Forward<IController>().ImplementedBy<EntryPointDescriptionController>().LifestyleTransient());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromQueryStringArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromUriArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromBodyArgumentBinder>().Activator<NonPublicComponentActivator>().LifestyleSingleton());
            container.Register(Component.For<IClassGenerator>().ImplementedBy<HydraClassGenerator>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<URSA.Web.Http.Description.CodeGen.GenericUriParser>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<HydraUriParser>().LifestyleSingleton().Named(typeof(HydraUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<XsdUriParser>().LifestyleSingleton().Named(typeof(XsdUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<OGuidUriParser>().LifestyleSingleton().Named(typeof(OGuidUriParser).FullName));
            container.Register(Component.For<IXmlDocProvider>().ImplementedBy<XmlDocProvider>().LifestyleSingleton());
            container.Register(Component.For<IWebRequestProvider>().ImplementedBy<WebRequestProvider>().LifestyleSingleton());
            container.Register(Component.For<IRequestHandler<RequestInfo, ResponseInfo>>().ImplementedBy<RequestHandler>().LifestyleSingleton());
            container.Register(Component.For<IResponseComposer>().ImplementedBy<ResponseComposer>().LifestyleSingleton());
            container.Register(Component.For<IDelegateMapper<RequestInfo>>().ImplementedBy<DelegateMapper>().LifestyleSingleton());
            container.Register(Component.For<IArgumentBinder<RequestInfo>>().ImplementedBy<ArgumentBinder>().LifestyleSingleton());
            container.Register(Component.For<IResultBinder<RequestInfo>>().ImplementedBy<ResultBinder>().LifestyleSingleton());
            container.Register(Component.For<ITypeDescriptionBuilder>().ImplementedBy<HydraCompliantTypeDescriptionBuilder>()
                .Named(EntityConverter.Hydra.ToString()).IsDefault().LifestyleSingleton());
            container.Register(Component.For<IServerBehaviorAttributeVisitor>().ImplementedBy<DescriptionBuildingServerBahaviorAttributeVisitor<ParameterInfo>>().Named("Hydra"));
            container.Register(Component.For(typeof(IApiDescriptionBuilder<>)).ImplementedBy(typeof(ApiDescriptionBuilder<>)).LifestyleSingleton().Forward<IApiDescriptionBuilder>());
            container.Register(Component.For<IApiEntryPointDescriptionBuilder>().ImplementedBy<ApiEntryPointDescriptionBuilder>().LifestyleSingleton().Forward<IApiDescriptionBuilder>());
            container.Register(Component.For<IApiDescriptionBuilderFactory>().AsFactory(new ApiDescriptiobBuilderTypedFactory()).LifestyleSingleton());
        }

        private IEntityContext CreateEntityContext(IKernel kernel, CreationContext context)
        {
            IEntityContext result = null;
            lock (_lock)
            {
                var baseUri = String.Format(
                    "{0}://{1}{2}/",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Host,
                    ((HttpContext.Current.Request.Url.Port != 80) && (HttpContext.Current.Request.Url.Port > 0) ? String.Format(":{0}", HttpContext.Current.Request.Url.Port) : String.Empty));
                result = _entityContextFactory.Value.WithDotNetRDF(new TripleStore()).WithBaseUri(policy => policy.Default.Is(new Uri(baseUri))).CreateContext();
            }

            return result;
        }

        private EntityContextFactory CreateEntityContextFactory()
        {
            return EntityContextFactory.FromConfiguration(EntityConverter.DefaultEntityContextFactoryName).WithDefaultOntologies();
        }
    }
}