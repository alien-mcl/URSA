#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Model;
using RomanticWeb.NamedGraphs;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Mapping;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Description.Rdfs;
using URSA.Web.Http.Description.Testing;
using URSA.Web.Http.Description.Tests;
using URSA.Web.Http.Description.Tests.Data;
using URSA.Web.Http.Tests.Testing;
using URSA.Web.Mapping;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiDescriptionBuilder_class
    {
        private static readonly Uri TemplatedLinkUri = new Uri(EntityConverter.Hydra + "TemplatedLink");
        private Mock<IEntityContext> _entityContext;
        private Mock<IApiDocumentation> _apiDocumentation;
        private Mock<IXmlDocProvider> _xmlDocProvider;
        private Mock<ITypeDescriptionBuilder> _typeDescriptionBuilder;
        private Mock<INamedGraphSelectorFactory> _namedGraphSelectorFactory;
        private IHttpControllerDescriptionBuilder<TestController> _descriptionBuilder;

        [TestMethod]
        public void it_should_build_the_api_documentation()
        {
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                _descriptionBuilder,
                _xmlDocProvider.Object,
                new[] { _typeDescriptionBuilder.Object },
                new IServerBehaviorAttributeVisitor[0],
                _namedGraphSelectorFactory.Object);
            apiDescriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            _apiDocumentation.Object.EntryPoints.Should().HaveCount(0);
            _apiDocumentation.Object.SupportedClasses.Should().HaveCount(1);
            _apiDocumentation.Object.SupportedClasses.First().SupportedOperations.Should().HaveCount(1);
            _apiDocumentation.Object.SupportedClasses.First().SupportedOperations.First().Method.Should().HaveCount(1);
            _apiDocumentation.Object.SupportedClasses.First().SupportedOperations.First().Method.First().Should().Be("POST");
        }

        [TestMethod]
        public void it_should_build_the_api_documentation_using_a_Hydra_profile()
        {
            Mock<ITypeDescriptionBuilder> shaclTypeDescriptionBuilder = SetupTypeDescriptionBuilder(_entityContext, EntityConverter.Shacl);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                _descriptionBuilder,
                _xmlDocProvider.Object,
                new[] { _typeDescriptionBuilder.Object, shaclTypeDescriptionBuilder.Object },
                new IServerBehaviorAttributeVisitor[0],
                _namedGraphSelectorFactory.Object);
            apiDescriptionBuilder.BuildDescription(_apiDocumentation.Object, new[] { EntityConverter.Hydra });

            _typeDescriptionBuilder.VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            shaclTypeDescriptionBuilder.VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            shaclTypeDescriptionBuilder.Verify(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>()), Times.Never);
        }

        [TestMethod]
        public void it_should_build_the_api_documentation_using_a_Shacl_profile()
        {
            Mock<ITypeDescriptionBuilder> shaclTypeDescriptionBuilder = SetupTypeDescriptionBuilder(_entityContext, EntityConverter.Shacl);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                _descriptionBuilder,
                _xmlDocProvider.Object,
                new[] { _typeDescriptionBuilder.Object, shaclTypeDescriptionBuilder.Object },
                new IServerBehaviorAttributeVisitor[0],
                _namedGraphSelectorFactory.Object);
            apiDescriptionBuilder.BuildDescription(_apiDocumentation.Object, new[] { EntityConverter.Shacl });

            _typeDescriptionBuilder.VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            shaclTypeDescriptionBuilder.VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            _typeDescriptionBuilder.Verify(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>()), Times.Never);
        }

        [TestMethod]
        public void it_should_determine_property_owning_an_operation()
        {
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                _descriptionBuilder,
                _xmlDocProvider.Object,
                new[] { _typeDescriptionBuilder.Object },
                new IServerBehaviorAttributeVisitor[0],
                _namedGraphSelectorFactory.Object);

            var operationOwner = apiDescriptionBuilder.DetermineOperationOwner(
                typeof(TestController).GetMethod("SetRoles").ToOperationInfo("http://temp.uri/", Verb.POST),
                DescriptionContext.ForType(_apiDocumentation.Object, typeof(Person), _typeDescriptionBuilder.Object),
                new Mock<IClass>(MockBehavior.Strict).Object);

            operationOwner.Should().BeAssignableTo<ISupportedProperty>();
            operationOwner.Id.ToString().Should().Be("urn:hydra:" + typeof(Person).FullName + ".Roles");
        }

        [TestMethod]
        public void it_should_list_possible_status_codes()
        {
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                _descriptionBuilder,
                _xmlDocProvider.Object,
                new[] { _typeDescriptionBuilder.Object },
                new IServerBehaviorAttributeVisitor[0],
                _namedGraphSelectorFactory.Object);

            apiDescriptionBuilder.BuildDescription(_apiDocumentation.Object, null);

            var statusCodes = _apiDocumentation.Object.SupportedClasses.First().GetSupportedOperations().First(operation => operation.Operation.Label == "Delete").Operation.StatusCodes;
            statusCodes.Should().Contain((int)HttpStatusCode.NoContent);
            statusCodes.Should().Contain((int)HttpStatusCode.NotFound);
            statusCodes.Should().Contain((int)HttpStatusCode.BadRequest);
            statusCodes.Should().Contain((int)HttpStatusCode.Unauthorized);
        }

        [TestInitialize]
        public void Setup()
        {
            Uri baseUri = new Uri("http://temp.org/");
            _apiDocumentation = MockHelpers.MockEntity<IApiDocumentation>((_entityContext = SetupEntityContext(baseUri)).Object, new EntityId(new Uri(baseUri, "api")));
            _xmlDocProvider = SetupXmlDocProvider();
            _typeDescriptionBuilder = SetupTypeDescriptionBuilder(_entityContext);
            _namedGraphSelectorFactory = SetupNamedGraphSelectorFactory(baseUri);
            _descriptionBuilder = SetupHttpControllerDescriptionBuilder();
        }

        [TestCleanup]
        public void Teardown()
        {
            _apiDocumentation = null;
            _xmlDocProvider = null;
            _typeDescriptionBuilder = null;
            _namedGraphSelectorFactory = null;
            _descriptionBuilder = null;
        }

        private static OperationInfo<Verb> CreateOperation(MethodInfo method, Verb verb)
        {
            string queryString = String.Empty;
            string uriTemplate = null;
            IList<ArgumentInfo> arguments = new List<ArgumentInfo>();
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.IsOut)
                {
                    continue;
                }

                string parameterTemplate = null;
                ParameterSourceAttribute source = null;
                if (parameter.ParameterType == typeof(Guid))
                {
                    source = FromUriAttribute.For(parameter);
                    uriTemplate += (parameterTemplate = "/" + parameter.Name + "/{?value}");
                }
                else if (parameter.ParameterType.IsValueType)
                {
                    source = FromQueryStringAttribute.For(parameter);
                    queryString += (parameterTemplate = "&" + parameter.Name + "={?value}");
                }
                else if (!parameter.ParameterType.IsValueType)
                {
                    source = FromBodyAttribute.For(parameter);
                }

                arguments.Add(new ArgumentInfo(parameter, source, parameterTemplate, (parameterTemplate != null ? parameter.Name : null)));
            }

            if (queryString.Length > 0)
            {
                uriTemplate += "?" + queryString.Substring(1);
            }

            return new OperationInfo<Verb>(method, new Uri("/", UriKind.Relative), uriTemplate, new Regex(".*"), verb, arguments.ToArray());
        }

        private static Mock<INamedGraphSelectorFactory> SetupNamedGraphSelectorFactory(Uri baseUri)
        {
            var namedGraphSelector = new Mock<INamedGraphSelector>(MockBehavior.Strict);
            namedGraphSelector.Setup(instance => instance.SelectGraph(It.IsAny<EntityId>(), It.IsAny<IEntityMapping>(), It.IsAny<IPropertyMapping>()))
                .Returns<EntityId, IEntityMapping, IPropertyMapping>((id, map, property) => baseUri);
            var namedGraphSelectorFactory = new Mock<INamedGraphSelectorFactory>(MockBehavior.Strict);
            namedGraphSelectorFactory.SetupGet(instance => instance.NamedGraphSelector).Returns(namedGraphSelector.Object);
            return namedGraphSelectorFactory;
        }

        private static Mock<IXmlDocProvider> SetupXmlDocProvider()
        {
            var deleteExceptions = new[] { "System.ArgumentNullException", "System.ArgumentOutOfRangeException", "URSA.Web.UnauthenticatedAccessException" };
            var xmlDocProvider = new Mock<IXmlDocProvider>(MockBehavior.Strict);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<MethodInfo>())).Returns<MethodInfo>(method => method.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<MethodInfo>(), It.IsAny<ParameterInfo>()))
                .Returns<MethodInfo, ParameterInfo>((method, parameter) => parameter.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<PropertyInfo>())).Returns<PropertyInfo>(property => property.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<Type>())).Returns<Type>(type => type.Name);
            xmlDocProvider.Setup(instance => instance.GetExceptions(It.IsAny<MethodInfo>()))
                .Returns<MethodInfo>(method => method.Name == "Delete" ? deleteExceptions : new string[0]);
            return xmlDocProvider;
        }

        private static IHttpControllerDescriptionBuilder<TestController> SetupHttpControllerDescriptionBuilder()
        {
            IDictionary<string, Verb> verbs = new Dictionary<string, Verb>()
            {
                { "Get", Verb.GET },
                { "List", Verb.GET },
                { "Create", Verb.POST },
                { "Update", Verb.PUT },
                { "Delete", Verb.DELETE },
                { "SetRoles", Verb.POST }
            };

            var operations = typeof(TestController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.DeclaringType != typeof(object))
                .Except(typeof(TestController).GetProperties(BindingFlags.Public | BindingFlags.Instance).SelectMany(property => new MethodInfo[] { property.GetGetMethod(), property.GetSetMethod() }))
                .Select(method => CreateOperation(method, verbs[method.Name]));

            ControllerInfo<TestController> controllerInfo = new ControllerInfo<TestController>(null, new Uri("/", UriKind.Relative), operations.ToArray());
            Mock<IHttpControllerDescriptionBuilder<TestController>> descriptionBuilder = new Mock<IHttpControllerDescriptionBuilder<TestController>>(MockBehavior.Strict);
            descriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.BuildDescriptor()).Returns(controllerInfo);
            descriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.GetMethodVerb(It.IsAny<MethodInfo>())).Returns<MethodInfo>(method => verbs[method.Name]);
            foreach (var operation in operations)
            {
                IEnumerable<ArgumentInfo> parameterMapping = operation.Arguments;
                descriptionBuilder.As<IHttpControllerDescriptionBuilder>().Setup(instance => instance.GetOperationUriTemplate(operation.UnderlyingMethod, out parameterMapping))
                    .Returns<MethodInfo, IEnumerable<ArgumentInfo>>((method, parameters) => operation.UriTemplate);
            }

            return descriptionBuilder.Object;
        }

        private static Mock<ITypeDescriptionBuilder> SetupTypeDescriptionBuilder(Mock<IEntityContext> entityContext, Uri supportedProfile = null)
        {
            bool requiresRdf;
            Mock<ITypeDescriptionBuilder> typeDescriptionBuilder = new Mock<ITypeDescriptionBuilder>(MockBehavior.Strict);
            typeDescriptionBuilder.SetupGet(instance => instance.SupportedProfiles)
                .Returns(new[] { supportedProfile ?? EntityConverter.Hydra });
            typeDescriptionBuilder.Setup(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>()))
                .Returns<DescriptionContext>(context => CreateDescription(entityContext, context, out requiresRdf));
            typeDescriptionBuilder.Setup(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>(), out requiresRdf))
                .Returns<DescriptionContext, bool>((context, rdf) => CreateDescription(entityContext, context, out rdf));
            typeDescriptionBuilder.Setup(instance => instance.SubClass(It.IsAny<DescriptionContext>(), It.IsAny<IClass>(), null))
                .Returns<DescriptionContext, IClass, EntityId>((context, @class, id) => @class);
            typeDescriptionBuilder.Setup(instance => instance.GetSupportedPropertyId(It.IsAny<PropertyInfo>(), It.IsAny<Type>()))
                .Returns<PropertyInfo, Type>((property, declaringType) => new EntityId(String.Format("urn:hydra:{0}.{1}", property.DeclaringType ?? declaringType, property.Name)));
            return typeDescriptionBuilder;
        }

        private static Mock<IEntityContext> SetupEntityContext(Uri baseUri)
        {
            var baseUriSelector = new Mock<IBaseUriSelectionPolicy>();
            baseUriSelector.Setup(instance => instance.SelectBaseUri(It.IsAny<EntityId>())).Returns(baseUri);

            var quads = new List<EntityQuad>();
            var store = new Mock<IEntityStore>(MockBehavior.Strict);
            store.SetupGet(instance => instance.Quads).Returns(quads);
            store.Setup(instance => instance.ReplacePredicateValues(It.IsAny<EntityId>(), It.IsAny<Node>(), It.IsAny<Func<IEnumerable<Node>>>(), It.IsAny<Uri>(), CultureInfo.InvariantCulture))
                .Callback<EntityId, Node, Func<IEnumerable<Node>>, Uri, CultureInfo>((id, node, nodes, uri, culture) =>
                    nodes().ForEach(item => quads.Add(new EntityQuad(id, Node.ForUri(id.Uri), node, item))));

            var @class = new Mock<IClassMapping>(MockBehavior.Strict);
            @class.SetupGet(instance => instance.Uri).Returns(TemplatedLinkUri);

            var entityMapping = new Mock<IEntityMapping>(MockBehavior.Strict);
            entityMapping.SetupGet(instance => instance.Classes).Returns(new[] { @class.Object });

            var mappingsRepository = new Mock<IMappingsRepository>(MockBehavior.Strict);
            mappingsRepository.Setup(instance => instance.MappingFor<ITemplatedLink>()).Returns(entityMapping.Object);

            var entities = new List<IEntity>();
            var entityContext = new Mock<IEntityContext>(MockBehavior.Strict);
            entityContext.SetupGet(instance => instance.Mappings).Returns(mappingsRepository.Object);
            entityContext.SetupGet(instance => instance.BaseUriSelector).Returns(baseUriSelector.Object);
            entityContext.SetupGet(instance => instance.Store).Returns(store.Object);
            entityContext.Setup(instance => instance.Create<ICreateResourceOperation>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (ICreateResourceOperation)entities.AddAndReturn(CreateOperation<ICreateResourceOperation>(entityContext, id)));
            entityContext.Setup(instance => instance.Create<IDeleteResourceOperation>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (IDeleteResourceOperation)entities.AddAndReturn(CreateOperation<IDeleteResourceOperation>(entityContext, id)));
            entityContext.Setup(instance => instance.Create<IReplaceResourceOperation>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (IReplaceResourceOperation)entities.AddAndReturn(CreateOperation<IReplaceResourceOperation>(entityContext, id)));
            entityContext.Setup(instance => instance.Create<IOperation>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (IOperation)entities.AddAndReturn(CreateOperation<IOperation>(entityContext, id)));
            entityContext.Setup(instance => instance.Create<IIriTemplateMapping>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (IIriTemplateMapping)entities.AddAndReturn(CreateIriTemplateMapping(entityContext, id)));
            entityContext.Setup(instance => instance.Create<IIriTemplate>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (IIriTemplate)entities.AddAndReturn(CreateIriTemplate(entityContext, id)));
            entityContext.Setup(instance => instance.Create<ITemplatedLink>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (ITemplatedLink)entities.AddAndReturn(CreateTemplatedLink(entityContext, id)));
            entityContext.Setup(instance => instance.Load<ISupportedProperty>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => (ISupportedProperty)entities.AddAndReturn(CreateProperty(entityContext, id)));
            entityContext.Setup(instance => instance.Load<IEntity>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => entities.First(item => item.Id == id));
            entityContext.Setup(instance => instance.Load<ITemplatedLink>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => entities.OfType<ITemplatedLink>().First(item => item.Id == id));
            entityContext.Setup(instance => instance.Load<IIriTemplate>(It.IsAny<EntityId>()))
                .Returns<EntityId>(id => entities.OfType<IIriTemplate>().First(item => item.Id == id));
            return entityContext;
        }

        private static IClass CreateDescription(Mock<IEntityContext> entityContext, DescriptionContext context, out bool requiresRdf)
        {
            var result = new Mock<IClass>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            result.SetupGet(instance => instance.Id).Returns(new EntityId(String.Format("urn:net:" + context.Type.FullName)));
            result.SetupSet(instance => instance.Label = It.IsAny<string>());
            result.SetupSet(instance => instance.Description = It.IsAny<string>());
            result.SetupGet(instance => instance.MediaTypes).Returns(new List<string>());
            if (context.Type == typeof(Person))
            {
                result.SetupGet(instance => instance.SupportedProperties).Returns(new ISupportedProperty[0]);
                result.SetupGet(instance => instance.SupportedOperations).Returns(new List<IOperation>());
            }

            context.Describe(result.Object, requiresRdf = false);
            return result.Object;
        }

        private static ISupportedProperty CreateProperty(Mock<IEntityContext> entityContext, EntityId id)
        {
            var result = new Mock<ISupportedProperty>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            result.SetupGet(instance => instance.Id).Returns(id);
            return result.Object;
        }

        private static T CreateOperation<T>(Mock<IEntityContext> entityContext, EntityId id) where T : class, IOperation
        {
            string label = null;
            var result = new Mock<T>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Method).Returns(new List<string>());
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupSet(instance => instance.Label = It.IsAny<string>()).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = It.IsAny<string>());
            result.SetupGet(instance => instance.MediaTypes).Returns(new List<string>());
            result.SetupGet(instance => instance.Returns).Returns(new List<IClass>());
            result.SetupGet(instance => instance.Expects).Returns(new List<IClass>());
            result.SetupGet(instance => instance.StatusCodes).Returns(new List<int>());
            return result.Object;
        }

        private static IIriTemplateMapping CreateIriTemplateMapping(Mock<IEntityContext> entityContext, EntityId id)
        {
            var result = new Mock<IIriTemplateMapping>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupSet(instance => instance.Variable = It.IsAny<string>());
            result.SetupSet(instance => instance.Description = It.IsAny<string>());
            result.SetupSet(instance => instance.Required = It.IsAny<bool>());
            result.SetupSet(instance => instance.Property = It.IsAny<IProperty>());
            return result.Object;
        }

        private static IIriTemplate CreateIriTemplate(Mock<IEntityContext> entityContext, EntityId id)
        {
            var result = new Mock<IIriTemplate>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupSet(instance => instance.Template = It.IsAny<string>());
            result.SetupGet(instance => instance.Mappings).Returns(new List<IIriTemplateMapping>());
            return result.Object;
        }

        private static ITemplatedLink CreateTemplatedLink(Mock<IEntityContext> entityContext, EntityId id)
        {
            var result = new Mock<ITemplatedLink>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.SupportedOperations).Returns(new List<IOperation>());
            result.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(TemplatedLinkUri) });
            return result.Object;
        }
    }
}