using System;
using System.Configuration;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using RDeF.Entities;
using URSA.CastleWindsor.ComponentModel;
using URSA.CodeGen;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Http.AutoFac.Description;
using URSA.Web;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.CastleWindsor
{
    /// <summary>Installs HTTP components.</summary>
    public class HttpInstaller : IWindsorInstaller
    {
        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            UrlParser.Register<HttpUrlParser>();
            UrlParser.Register<FtpUrlParser>();
            container.AddFacility<TypedFactoryFacility>();
            var typedFactory = new UrsaCustomTypedFactory();
            InstallRdfDependencies(container, typedFactory);
            InstallRequestPipelineDependencies(container, typedFactory);
            InstallDescriptionDependencies(container, typedFactory);
        }

        private void InstallRequestPipelineDependencies(IWindsorContainer container, UrsaCustomTypedFactory typedFactory)
        {
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultValueRelationSelectorType != null) ?
                configuration.DefaultValueRelationSelectorType :
                typeof(DefaultValueRelationSelector));
            container.Register(Component.For<IDelegateMapper<RequestInfo>>().ImplementedBy<DelegateMapper>().LifestyleScoped());
            container.Register(Component.For<IDefaultValueRelationSelector>().ImplementedBy(sourceSelectorType).LifestyleSingleton());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromQueryStringArgumentBinder>()
                .Activator<NonPublicComponentActivator>().LifestyleScoped());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromUrlArgumentBinder>()
                .Activator<NonPublicComponentActivator>().LifestyleScoped());
            container.Register(Component.For<IParameterSourceArgumentBinder>().ImplementedBy<FromBodyArgumentBinder>()
                .Activator<NonPublicComponentActivator>().LifestyleScoped());
            container.Register(Component.For<IWebRequestProvider>().ImplementedBy<WebRequestProvider>().LifestyleSingleton());
            container.Register(Component.For<IRequestHandler<RequestInfo, ResponseInfo>>().ImplementedBy<RequestHandler>().LifestyleScoped());
            container.Register(Component.For<IResponseComposer>().ImplementedBy<ResponseComposer>().LifestyleScoped());
            container.Register(Component.For<IArgumentBinder<RequestInfo>>().ImplementedBy<ArgumentBinder>().LifestyleScoped());
            container.Register(Component.For<IResultBinder<RequestInfo>>().ImplementedBy<ResultBinder>().LifestyleScoped());
            container.Register(Component.For<IResponseModelTransformer>().ImplementedBy<RdfPayloadModelTransformer>()
                .Named("RdfPayloadRequestModelTransformer").LifestyleScoped());
            container.Register(Component.For<IResponseModelTransformer>().ImplementedBy<CollectionResponseModelTransformer>()
                .Named("CollectionResponseModelTransformer").LifestyleScoped());
            container.Register(Component.For<IRequestModelTransformer>().ImplementedBy<RdfPayloadModelTransformer>()
                .Named("RdfPayloadResponseModelTransformer").LifestyleScoped());
        }

        private void InstallRdfDependencies(IWindsorContainer container, UrsaCustomTypedFactory typedFactory)
        {
            container.Register(Component.For<IEntityContextFactory>()
                .UsingFactoryMethod(kernel => EntityContextFactory.FromConfiguration("in-memory"))
                .Named("InMemoryEntityContextFactory")
                .LifestyleSingleton());
            container.Register(Component.For<IEntityContext>()
                .UsingFactoryMethod(kernel => kernel.Resolve<IEntityContextFactory>().Create())
                .Named("InMemoryEntityContext")
                .LifestyleScoped());
        }

        private void InstallDescriptionDependencies(IWindsorContainer container, UrsaCustomTypedFactory typedFactory)
        {
            WindsorComponentProvider componentProvider = container.Resolve<WindsorComponentProvider>();
            container.Register(Component.For(typeof(DescriptionController<>)).Forward<IController>()
                .ImplementedBy(typeof(DescriptionController<>), componentProvider.GenericImplementationMatchingStrategy).LifestyleTransient());
            container.Register(Component.For<EntryPointDescriptionController>().Forward<IController>().ImplementedBy<EntryPointDescriptionController>().LifestyleTransient());
            container.Register(Component.For<ITypeDescriptionBuilder>().ImplementedBy<HydraCompliantTypeDescriptionBuilder>()
                .Named(EntityConverter.Hydra.ToString()).IsDefault().LifestyleSingleton());
            container.Register(Component.For<IServerBehaviorAttributeVisitor>().ImplementedBy<DescriptionBuildingServerBahaviorAttributeVisitor<ParameterInfo>>().Named("Hydra"));
            container.Register(Component.For<IApiEntryPointDescriptionBuilder>().ImplementedBy<ApiEntryPointDescriptionBuilder>().Forward<IApiDescriptionBuilder>().LifestyleSingleton());
            container.Register(Component.For<IApiDescriptionBuilderFactory>().UsingFactoryMethod(kernel =>
                new DefaultApiDescriptionBuilderFactory(type => (IApiDescriptionBuilder)kernel.Resolve(typeof(IApiDescriptionBuilder<>).MakeGenericType(type)))).LifestyleSingleton());
            container.Register(Component.For<IClassGenerator>().ImplementedBy<HydraClassGenerator>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<Web.Http.Description.CodeGen.GenericUriParser>().LifestyleSingleton());
            container.Register(Component.For<IUriParser>().ImplementedBy<HydraUriParser>().LifestyleSingleton().Named(typeof(HydraUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<XsdUriParser>().LifestyleSingleton().Named(typeof(XsdUriParser).FullName));
            container.Register(Component.For<IUriParser>().ImplementedBy<OGuidUriParser>().LifestyleSingleton().Named(typeof(OGuidUriParser).FullName));
            container.Register(Component.For<IXmlDocProvider>().ImplementedBy<XmlDocProvider>().LifestyleSingleton());
        }
    }
}