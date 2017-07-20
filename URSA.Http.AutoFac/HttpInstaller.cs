using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using RDeF.Entities;
using URSA.CodeGen;
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
using GenericUriParser = URSA.Web.Http.Description.CodeGen.GenericUriParser;

namespace URSA.AutoFac
{
    /// <summary>Installs HTTP components.</summary>
    public class HttpInstaller : Autofac.Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            UrlParser.Register<HttpUrlParser>();
            UrlParser.Register<FtpUrlParser>();
            InstallRdfDependencies(builder);
            InstallRequestPipelineDependencies(builder);
            InstallDescriptionDependencies(builder);
        }

        private void InstallRequestPipelineDependencies(ContainerBuilder builder)
        {
            var configuration = (HttpConfigurationSection)ConfigurationManager.GetSection(HttpConfigurationSection.ConfigurationSection);
            Type sourceSelectorType = ((configuration != null) && (configuration.DefaultValueRelationSelectorType != null) ?
                configuration.DefaultValueRelationSelectorType :
                typeof(DefaultValueRelationSelector));
            builder.RegisterType<DelegateMapper>().As<IDelegateMapper<RequestInfo>>().InstancePerLifetimeScope();
            builder.RegisterType(sourceSelectorType).As<IDefaultValueRelationSelector>().SingleInstance();
            builder.RegisterType<FromQueryStringArgumentBinder>().As<IParameterSourceArgumentBinder>()
                .FindConstructorsWith(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)).InstancePerLifetimeScope();
            builder.RegisterType<FromUrlArgumentBinder>().As<IParameterSourceArgumentBinder>()
                .FindConstructorsWith(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)).InstancePerLifetimeScope();
            builder.RegisterType<FromBodyArgumentBinder>().As<IParameterSourceArgumentBinder>()
                .FindConstructorsWith(type => type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)).InstancePerLifetimeScope();
            builder.RegisterType<WebRequestProvider>().As<IWebRequestProvider>().SingleInstance();
            builder.RegisterType<RequestHandler>().As<IRequestHandler<RequestInfo, ResponseInfo>>().InstancePerLifetimeScope();
            builder.RegisterType<ResponseComposer>().As<IResponseComposer>().InstancePerLifetimeScope();
            builder.RegisterType<ArgumentBinder>().As<IArgumentBinder<RequestInfo>>().InstancePerLifetimeScope();
            builder.RegisterType<ResultBinder>().As<IResultBinder<RequestInfo>>().InstancePerLifetimeScope();
            builder.RegisterType<RdfPayloadModelTransformer>().As<IResponseModelTransformer>()
                .Named<IResponseModelTransformer>("RdfPayloadResponseModelTransformer").InstancePerLifetimeScope();
            builder.RegisterType<CollectionResponseModelTransformer>().As<IResponseModelTransformer>()
                .Named<IResponseModelTransformer>("CollectionResponseModelTransformer").InstancePerLifetimeScope();
            builder.RegisterType<RdfPayloadModelTransformer>().As<IRequestModelTransformer>()
                .Named<IRequestModelTransformer>("RdfPayloadRequestModelTransformer").InstancePerLifetimeScope();
        }

        private void InstallRdfDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultEntityContextFactory>().Named<IEntityContextFactory>("InMemoryEntityContextFactory").SingleInstance();
            builder.Register(context => context.ResolveNamed<IEntityContextFactory>("InMemoryEntityContextFactory").Create())
                .Named<IEntityContext>("InMemoryEntityContext").InstancePerLifetimeScope();
        }

        private void InstallDescriptionDependencies(ContainerBuilder builder)
        {
            builder.RegisterType(typeof(DescriptionController<>)).As(typeof(DescriptionController<>)).As<IController>().InstancePerDependency();
            builder.RegisterType<EntryPointDescriptionController>().As(typeof(EntryPointDescriptionController)).As<IController>().InstancePerDependency();
            builder.RegisterType<HydraCompliantTypeDescriptionBuilder>().As<ITypeDescriptionBuilder>()
                .Named<ITypeDescriptionBuilder>(EntityConverter.Hydra.ToString()).SingleInstance();
            builder.RegisterType<DescriptionBuildingServerBahaviorAttributeVisitor<ParameterInfo>>().As<IServerBehaviorAttributeVisitor>()
                .Named<IServerBehaviorAttributeVisitor>("Hydra");
            builder.RegisterType<ApiEntryPointDescriptionBuilder>().As<IApiEntryPointDescriptionBuilder>().As<IApiDescriptionBuilder>().SingleInstance();
            builder.Register(context =>
                {
                    var childContext = context.Resolve<IComponentContext>();
                    return new DefaultApiDescriptionBuilderFactory(
                        type => (IApiDescriptionBuilder)childContext.Resolve(typeof(IApiDescriptionBuilder<>).MakeGenericType(type)));
                })
                .As<IApiDescriptionBuilderFactory>().SingleInstance();
            builder.RegisterType<HydraClassGenerator>().As<IClassGenerator>().SingleInstance();
            builder.RegisterType<GenericUriParser>().As<IUriParser>().SingleInstance();
            builder.RegisterType<HydraUriParser>().As<IUriParser>().Named<IUriParser>(typeof(HydraUriParser).FullName).SingleInstance();
            builder.RegisterType<XsdUriParser>().As<IUriParser>().Named<IUriParser>(typeof(XsdUriParser).FullName).SingleInstance();
            builder.RegisterType<OGuidUriParser>().As<IUriParser>().Named<IUriParser>(typeof(OGuidUriParser).FullName).SingleInstance();
            builder.RegisterType<XmlDocProvider>().As<IXmlDocProvider>().SingleInstance();
        }
    }
}