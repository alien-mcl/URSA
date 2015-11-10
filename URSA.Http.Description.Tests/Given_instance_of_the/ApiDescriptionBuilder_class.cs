﻿#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
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
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiDescriptionBuilder_class
    {
        [TestMethod]
        public void it_should_build_the_api_documentation()
        {
            IApiDocumentation apiDocumentation;
            IXmlDocProvider xmlDocProvider;
            ITypeDescriptionBuilder typeDescriptionBuilder;
            INamedGraphSelectorFactory namedGraphSelectorFactory;
            var descriptionBuilder = SetupInfrastucture(out apiDocumentation, out xmlDocProvider, out typeDescriptionBuilder, out namedGraphSelectorFactory);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                descriptionBuilder,
                xmlDocProvider,
                new[] { typeDescriptionBuilder },
                new IServerBehaviorAttributeVisitor[0],
                namedGraphSelectorFactory);
            apiDescriptionBuilder.BuildDescription(apiDocumentation, null);

            apiDocumentation.EntryPoints.Should().HaveCount(0);
            apiDocumentation.SupportedClasses.Should().HaveCount(1);
            apiDocumentation.SupportedClasses.First().SupportedOperations.Should().HaveCount(1);
            apiDocumentation.SupportedClasses.First().SupportedOperations.First().Method.Should().HaveCount(1);
            apiDocumentation.SupportedClasses.First().SupportedOperations.First().Method.First().Should().Be("POST");
        }

        [TestMethod]
        public void it_should_build_the_api_documentation_using_a_Hydra_profile()
        {
            IApiDocumentation apiDocumentation;
            IXmlDocProvider xmlDocProvider;
            ITypeDescriptionBuilder shaclTypeDescriptionBuilder = SetupTypeDescriptionBuilder(EntityConverter.Shacl);
            ITypeDescriptionBuilder hydraTypeDescriptionBuilder;
            INamedGraphSelectorFactory namedGraphSelectorFactory;
            var descriptionBuilder = SetupInfrastucture(out apiDocumentation, out xmlDocProvider, out hydraTypeDescriptionBuilder, out namedGraphSelectorFactory);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                descriptionBuilder,
                xmlDocProvider,
                new[] { hydraTypeDescriptionBuilder, shaclTypeDescriptionBuilder },
                new IServerBehaviorAttributeVisitor[0],
                namedGraphSelectorFactory);
            apiDescriptionBuilder.BuildDescription(apiDocumentation, new[] { EntityConverter.Hydra });

            Mock.Get(hydraTypeDescriptionBuilder).VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            Mock.Get(shaclTypeDescriptionBuilder).VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            Mock.Get(shaclTypeDescriptionBuilder).Verify(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>()), Times.Never);
        }

        [TestMethod]
        public void it_should_build_the_api_documentation_using_a_Shacl_profile()
        {
            IApiDocumentation apiDocumentation;
            IXmlDocProvider xmlDocProvider;
            ITypeDescriptionBuilder shaclTypeDescriptionBuilder = SetupTypeDescriptionBuilder(EntityConverter.Shacl);
            ITypeDescriptionBuilder hydraTypeDescriptionBuilder;
            INamedGraphSelectorFactory namedGraphSelectorFactory;
            var descriptionBuilder = SetupInfrastucture(out apiDocumentation, out xmlDocProvider, out hydraTypeDescriptionBuilder, out namedGraphSelectorFactory);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                descriptionBuilder,
                xmlDocProvider,
                new[] { hydraTypeDescriptionBuilder, shaclTypeDescriptionBuilder },
                new IServerBehaviorAttributeVisitor[0],
                namedGraphSelectorFactory);
            apiDescriptionBuilder.BuildDescription(apiDocumentation, new[] { EntityConverter.Shacl });

            Mock.Get(hydraTypeDescriptionBuilder).VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            Mock.Get(shaclTypeDescriptionBuilder).VerifyGet(instance => instance.SupportedProfiles, Times.Once);
            Mock.Get(hydraTypeDescriptionBuilder).Verify(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>()), Times.Never);
        }

        [TestMethod]
        public void it_should_determine_property_owning_an_operation()
        {
            IApiDocumentation apiDocumentation;
            IXmlDocProvider xmlDocProvider;
            ITypeDescriptionBuilder typeDescriptionBuilder;
            INamedGraphSelectorFactory namedGraphSelectorFactory;
            var descriptionBuilder = SetupInfrastucture(out apiDocumentation, out xmlDocProvider, out typeDescriptionBuilder, out namedGraphSelectorFactory);
            var apiDescriptionBuilder = new ApiDescriptionBuilder<TestController>(
                descriptionBuilder,
                xmlDocProvider,
                new[] { typeDescriptionBuilder },
                new IServerBehaviorAttributeVisitor[0],
                namedGraphSelectorFactory);

            var operationOwner = apiDescriptionBuilder.DetermineOperationOwner(
                typeof(TestController).GetMethod("SetRoles").ToOperationInfo("http://temp.uri/", Verb.POST),
                DescriptionContext.ForType(apiDocumentation, typeof(Person), typeDescriptionBuilder),
                new Mock<IClass>(MockBehavior.Strict).Object);

            operationOwner.Should().BeAssignableTo<ISupportedProperty>();
            operationOwner.Id.ToString().Should().Be("urn:hydra:" + typeof(Person).FullName + ".Roles");
        }

        private static IHttpControllerDescriptionBuilder<TestController> SetupInfrastucture(
            out IApiDocumentation apiDocumentationInstance,
            out IXmlDocProvider xmlDocProvider,
            out ITypeDescriptionBuilder typeDescriptionBuilder,
            out INamedGraphSelectorFactory namedGraphSelectorFactory)
        {
            Uri baseUri = new Uri("http://temp.org/");
            apiDocumentationInstance = MockHelpers.MockEntity<IApiDocumentation>(SetupEntityContext(baseUri), new EntityId(new Uri(baseUri, "api"))).Object;
            xmlDocProvider = SetupXmlDocProvider();
            typeDescriptionBuilder = SetupTypeDescriptionBuilder();
            namedGraphSelectorFactory = SetupNamedGraphSelectorFactory(baseUri);
            return SetupHttpControllerDescriptionBuilder();
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

        private static INamedGraphSelectorFactory SetupNamedGraphSelectorFactory(Uri baseUri)
        {
            var namedGraphSelector = new Mock<INamedGraphSelector>(MockBehavior.Strict);
            namedGraphSelector.Setup(instance => instance.SelectGraph(It.IsAny<EntityId>(), It.IsAny<IEntityMapping>(), It.IsAny<IPropertyMapping>()))
                .Returns<EntityId, IEntityMapping, IPropertyMapping>((id, map, property) => baseUri);
            var namedGraphSelectorFactory = new Mock<INamedGraphSelectorFactory>(MockBehavior.Strict);
            namedGraphSelectorFactory.SetupGet(instance => instance.NamedGraphSelector).Returns(namedGraphSelector.Object);
            return namedGraphSelectorFactory.Object;
        }

        private static IXmlDocProvider SetupXmlDocProvider()
        {
            var xmlDocProvider = new Mock<IXmlDocProvider>(MockBehavior.Strict);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<MethodInfo>())).Returns<MethodInfo>(method => method.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<MethodInfo>(), It.IsAny<ParameterInfo>())).Returns<MethodInfo, ParameterInfo>((method, parameter) => parameter.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<PropertyInfo>())).Returns<PropertyInfo>(property => property.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<Type>())).Returns<Type>(type => type.Name);
            return xmlDocProvider.Object;
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

        private static ITypeDescriptionBuilder SetupTypeDescriptionBuilder(Uri supportedProfile = null)
        {
            bool requiresRdf;
            Mock<ITypeDescriptionBuilder> typeDescriptionBuilder = new Mock<ITypeDescriptionBuilder>(MockBehavior.Strict);
            typeDescriptionBuilder.SetupGet(instance => instance.SupportedProfiles)
                .Returns(new[] { supportedProfile ?? EntityConverter.Hydra });
            typeDescriptionBuilder.Setup(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>()))
                .Returns<DescriptionContext>(context => CreateDescription(context, out requiresRdf));
            typeDescriptionBuilder.Setup(instance => instance.BuildTypeDescription(It.IsAny<DescriptionContext>(), out requiresRdf))
                .Returns<DescriptionContext, bool>((context, rdf) => CreateDescription(context, out rdf));
            typeDescriptionBuilder.Setup(instance => instance.SubClass(It.IsAny<DescriptionContext>(), It.IsAny<IClass>(), null))
                .Returns<DescriptionContext, IClass, EntityId>((context, @class, id) => @class);
            typeDescriptionBuilder.Setup(instance => instance.GetSupportedPropertyId(It.IsAny<PropertyInfo>(), It.IsAny<Type>()))
                .Returns<PropertyInfo, Type>((property, declaringType) => new EntityId(String.Format("urn:hydra:{0}.{1}", property.DeclaringType ?? declaringType, property.Name)));
            return typeDescriptionBuilder.Object;
        }

        private static IEntityContext SetupEntityContext(Uri baseUri)
        {
            var baseUriSelector = new Mock<IBaseUriSelectionPolicy>();
            baseUriSelector.Setup(instance => instance.SelectBaseUri(It.IsAny<EntityId>())).Returns(baseUri);

            var store = new Mock<IEntityStore>(MockBehavior.Strict);
            store.Setup(instance => instance.ReplacePredicateValues(It.IsAny<EntityId>(), It.IsAny<Node>(), It.IsAny<Func<IEnumerable<Node>>>(), It.IsAny<Uri>(), CultureInfo.InvariantCulture));

            var entityContext = new Mock<IEntityContext>(MockBehavior.Strict);
            entityContext.SetupGet(instance => instance.BaseUriSelector).Returns(baseUriSelector.Object);
            entityContext.SetupGet(instance => instance.Store).Returns(store.Object);
            entityContext.Setup(instance => instance.Create<ICreateResourceOperation>(It.IsAny<EntityId>())).Returns<EntityId>(CreateOperation<ICreateResourceOperation>);
            entityContext.Setup(instance => instance.Create<IDeleteResourceOperation>(It.IsAny<EntityId>())).Returns<EntityId>(CreateOperation<IDeleteResourceOperation>);
            entityContext.Setup(instance => instance.Create<IReplaceResourceOperation>(It.IsAny<EntityId>())).Returns<EntityId>(CreateOperation<IReplaceResourceOperation>);
            entityContext.Setup(instance => instance.Create<IOperation>(It.IsAny<EntityId>())).Returns<EntityId>(CreateOperation<IOperation>);
            entityContext.Setup(instance => instance.Create<IIriTemplateMapping>(It.IsAny<EntityId>())).Returns<EntityId>(CreateIriTemplateMapping);
            entityContext.Setup(instance => instance.Create<IIriTemplate>(It.IsAny<EntityId>())).Returns<EntityId>(CreateIriTemplate);
            entityContext.Setup(instance => instance.Create<ITemplatedLink>(It.IsAny<EntityId>())).Returns<EntityId>(CreateTemplatedLink);
            entityContext.Setup(instance => instance.Load<ISupportedProperty>(It.IsAny<EntityId>())).Returns<EntityId>(CreateProperty);
            return entityContext.Object;
        }

        private static IClass CreateDescription(DescriptionContext context, out bool requiresRdf)
        {
            var result = new Mock<IClass>(MockBehavior.Strict);
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

        private static ISupportedProperty CreateProperty(EntityId id)
        {
            var result = new Mock<ISupportedProperty>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Id).Returns(id);
            return result.Object;
        }

        private static T CreateOperation<T>(EntityId id) where T : class, IOperation
        {
            var result = new Mock<T>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Method).Returns(new List<string>());
            result.SetupSet(instance => instance.Label = It.IsAny<string>());
            result.SetupSet(instance => instance.Description = It.IsAny<string>());
            result.SetupGet(instance => instance.MediaTypes).Returns(new List<string>());
            result.SetupGet(instance => instance.Returns).Returns(new List<IClass>());
            result.SetupGet(instance => instance.Expects).Returns(new List<IClass>());
            return result.Object;
        }

        private static IIriTemplateMapping CreateIriTemplateMapping(EntityId id)
        {
            var result = new Mock<IIriTemplateMapping>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupSet(instance => instance.Variable = It.IsAny<string>());
            result.SetupSet(instance => instance.Description = It.IsAny<string>());
            result.SetupSet(instance => instance.Required = It.IsAny<bool>());
            result.SetupSet(instance => instance.Property = It.IsAny<IProperty>());
            return result.Object;
        }

        private static IIriTemplate CreateIriTemplate(EntityId id)
        {
            var result = new Mock<IIriTemplate>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupSet(instance => instance.Template = It.IsAny<string>());
            result.SetupGet(instance => instance.Mappings).Returns(new List<IIriTemplateMapping>());
            return result.Object;
        }

        private static ITemplatedLink CreateTemplatedLink(EntityId id)
        {
            var result = new Mock<ITemplatedLink>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.SupportedOperations).Returns(new List<IOperation>());
            return result.Object;
        }
    }
}