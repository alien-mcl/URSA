#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Vocabularies;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using IClass = URSA.Web.Http.Description.Hydra.IClass;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace Given_instance_of_the.HydraCompliantTypeDescriptionBuilder_class
{
    [TestClass]
    public class HydraCompliantTypeDescriptionBuilderTest<T>
    {
        protected IApiDocumentation ApiDocumentation { get; private set; }

        protected HydraCompliantTypeDescriptionBuilder Builder { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            var xmlDocProvider = new Mock<IXmlDocProvider>(MockBehavior.Strict);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<Type>())).Returns<Type>(type => type.FullName);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<PropertyInfo>())).Returns<PropertyInfo>(property => typeof(T) + "." + property.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<MethodInfo>())).Returns<MethodInfo>(method => method.DeclaringType + "." + method.Name + "()");
            ApiDocumentation = CreateApiDocumentation();
            Builder = new HydraCompliantTypeDescriptionBuilder(xmlDocProvider.Object);
        }

        [TestCleanup]
        public void Teardown()
        {
            Builder = null;
            ApiDocumentation = null;
        }

        private static IApiDocumentation CreateApiDocumentation()
        {
            var blankIdGenerator = new Mock<IBlankNodeIdGenerator>(MockBehavior.Strict);
            blankIdGenerator.Setup(instance => instance.Generate()).Returns("bnode" + new Random().Next());
            var collectionClassMapping = new Mock<IClassMapping>(MockBehavior.Strict);
            collectionClassMapping.SetupGet(instance => instance.Uri).Returns(new Uri(EntityConverter.Hydra.AbsoluteUri + "Collection"));
            var managesMapping = new Mock<IPropertyMapping>(MockBehavior.Strict);
            managesMapping.SetupGet(instance => instance.Uri).Returns(new Uri(EntityConverter.Hydra.AbsoluteUri + "member"));
            var collectionMapping = new Mock<IEntityMapping>(MockBehavior.Strict);
            collectionMapping.SetupGet(instance => instance.Classes).Returns(new[] { collectionClassMapping.Object });
            collectionMapping.Setup(instance => instance.PropertyFor("Members")).Returns(managesMapping.Object);
            var mappings = new Mock<IMappingsRepository>(MockBehavior.Strict);
            mappings.Setup(instance => instance.MappingFor<ICollection>()).Returns(collectionMapping.Object);
            var entityContext = new Mock<IEntityContext>(MockBehavior.Strict);
            entityContext.SetupGet(instance => instance.BlankIdGenerator).Returns(blankIdGenerator.Object);
            entityContext.SetupGet(instance => instance.Mappings).Returns(mappings.Object);
            entityContext.Setup(instance => instance.Create<IClass>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateClass(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<ISupportedProperty>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateSupportedProperty(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<IProperty>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateProperty<IProperty>(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<IInverseFunctionalProperty>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateProperty<IInverseFunctionalProperty>(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<IRestriction>(It.IsAny<EntityId>())).Returns<EntityId>(id => CreateRestriction(entityContext.Object, id));
            var result = new Mock<IApiDocumentation>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Id).Returns(new EntityId("http://temp.uri/api"));
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            return result.Object;
        }

        private static IResource CreateResource(IEntityContext entityContext, EntityId id)
        {
            var result = new Mock<IResource>(MockBehavior.Strict);
            string label = null;
            string description = null;
            result.SetupSet(instance => instance.Label = It.IsAny<string>()).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = It.IsAny<string>()).Callback<string>(value => description = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupGet(instance => instance.Description).Returns(() => description);
            return result.Object;
        }

        private static IClass CreateClass(IEntityContext entityContext, EntityId id)
        {
            var result = new Mock<IClass>(MockBehavior.Strict);
            result.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(Rdfs.Class), new EntityId(new Uri(EntityConverter.Hydra.AbsoluteUri + "Class")),  });
            string label = null;
            string description = null;
            result.SetupSet(instance => instance.Label = It.IsAny<string>()).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = It.IsAny<string>()).Callback<string>(value => description = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupGet(instance => instance.Description).Returns(() => description);
            result.SetupGet(instance => instance.SupportedProperties).Returns(new List<ISupportedProperty>());
            result.SetupGet(instance => instance.SubClassOf).Returns(new List<URSA.Web.Http.Description.Rdfs.IClass>());
            return result.Object;
        }

        private static ISupportedProperty CreateSupportedProperty(IEntityContext entityContext, EntityId id)
        {
            var propertyFullName = id.Uri.AbsoluteUri.Substring(id.Uri.AbsoluteUri.LastIndexOf(':') + 1);
            var declaringTypeName = propertyFullName.Substring(0, propertyFullName.LastIndexOf('.'));
            var propertyName = propertyFullName.Substring(declaringTypeName.Length + 1);
            var propertyInfo = Type.GetType(declaringTypeName).GetProperty(propertyName);
            var readOnly = propertyInfo.CanRead;
            var writeOnly = propertyInfo.CanWrite;
            var required = (propertyInfo.PropertyType.IsValueType) || (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null);
            var result = new Mock<ISupportedProperty>(MockBehavior.Strict);
            IProperty property = null;
            result.SetupSet(instance => instance.Readable = It.Is<bool>(value => value == readOnly));
            result.SetupSet(instance => instance.Writeable = It.Is<bool>(value => value == writeOnly));
            result.SetupSet(instance => instance.Required = It.Is<bool>(value => value == required));
            result.SetupSet(instance => instance.Property = It.Is<IProperty>(value => value.Id.Uri.AbsoluteUri.Contains(propertyFullName))).Callback<IProperty>(value => property = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Readable).Returns(readOnly);
            result.SetupGet(instance => instance.Writeable).Returns(writeOnly);
            result.SetupGet(instance => instance.Required).Returns(required);
            result.SetupGet(instance => instance.Property).Returns(() => property);
            return result.Object;
        }

        private static TProperty CreateProperty<TProperty>(IEntityContext entityContext, EntityId id) where TProperty : class, IProperty
        {
            string propertyFullName = (id.Uri.AbsoluteUri == EntityConverter.Hydra.AbsoluteUri + "member" ? 
                String.Format("{0}.Members", typeof(ICollection).FullName) : 
                id.Uri.AbsoluteUri.Substring(id.Uri.AbsoluteUri.LastIndexOf(':') + 1));
            var declaringTypeName = propertyFullName.Substring(0, propertyFullName.LastIndexOf('.'));
            var propertyName = propertyFullName.Substring(declaringTypeName.Length + 1);
            var propertyInfo = (propertyName == "Members" ? typeof(ICollection) : Type.GetType(declaringTypeName)).GetProperty(propertyName);
            var result = new Mock<TProperty>(MockBehavior.Strict);
            string label = null;
            string description = null;
            result.SetupSet(instance => instance.Label = propertyInfo.Name).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = propertyFullName).Callback<string>(value => description = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupGet(instance => instance.Description).Returns(() => description);
            result.SetupGet(instance => instance.Domain).Returns(new List<URSA.Web.Http.Description.Rdfs.IClass>());
            result.SetupGet(instance => instance.Range).Returns(new List<URSA.Web.Http.Description.Rdfs.IResource>());
            return result.Object;
        }

        private static IRestriction CreateRestriction(IEntityContext entityContext, EntityId id)
        {
            var result = new Mock<IRestriction>(MockBehavior.Strict);
            result.As<ITypedEntity>().SetupGet(instance => instance.Types).Returns(new[] { new EntityId(Owl.Restriction) });
            IProperty property = null;
            IEntity allValuesFrom = null;
            uint maxCardinality = 0;
            result.SetupSet(instance => instance.OnProperty = It.IsAny<IProperty>()).Callback<IProperty>(value => property = value);
            result.SetupSet(instance => instance.MaxCardinality = It.IsAny<uint>()).Callback<uint>(value => maxCardinality = value);
            result.SetupSet(instance => instance.AllValuesFrom = It.IsAny<IEntity>()).Callback<IEntity>(value => allValuesFrom = value);
            result.SetupGet(instance => instance.AllValuesFrom).Returns(() => allValuesFrom);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.MaxCardinality).Returns(() => maxCardinality);
            result.SetupGet(instance => instance.OnProperty).Returns(() => property);
            result.SetupGet(instance => instance.SubClassOf).Returns(new List<URSA.Web.Http.Description.Rdfs.IClass>());
            return result.Object;
        }
    }
}