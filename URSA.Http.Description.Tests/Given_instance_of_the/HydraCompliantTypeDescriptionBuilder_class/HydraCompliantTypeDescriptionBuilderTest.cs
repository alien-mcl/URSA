#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Moq;
using NUnit.Framework;
using RDeF.Entities;
using RDeF.Mapping;
using RDeF.Vocabularies;
using RollerCaster;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using IClass = URSA.Web.Http.Description.Hydra.IClass;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace Given_instance_of_the.HydraCompliantTypeDescriptionBuilder_class
{
    public class HydraCompliantTypeDescriptionBuilderTest<T>
    {
        protected IApiDocumentation ApiDocumentation { get; private set; }

        protected HydraCompliantTypeDescriptionBuilder Builder { get; private set; }

        [SetUp]
        public void Setup()
        {
            var xmlDocProvider = new Mock<IXmlDocProvider>(MockBehavior.Strict);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<Type>())).Returns<Type>(type => type.FullName);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<PropertyInfo>())).Returns<PropertyInfo>(property => typeof(T) + "." + property.Name);
            xmlDocProvider.Setup(instance => instance.GetDescription(It.IsAny<MethodInfo>())).Returns<MethodInfo>(method => method.DeclaringType + "." + method.Name + "()");
            ApiDocumentation = CreateApiDocumentation();
            Builder = new HydraCompliantTypeDescriptionBuilder(xmlDocProvider.Object);
        }

        [TearDown]
        public void Teardown()
        {
            Builder = null;
            ApiDocumentation = null;
        }

        private static IApiDocumentation CreateApiDocumentation()
        {
            var mappings = new Mock<IMappingsRepository>(MockBehavior.Strict);
            SetupEntityMapping<IClass>(mappings, new Iri(EntityConverter.Hydra.AbsoluteUri + "Class"));
            SetupEntityMapping<ICollection>(mappings, new Iri(EntityConverter.Hydra.AbsoluteUri + "Collection"), new Dictionary<string, Iri>() { { "Members", new Iri(EntityConverter.Hydra.AbsoluteUri + "member") } });
            SetupEntityMapping<IRestriction>(mappings, owl.Restriction);
            var entityContext = new Mock<IEntityContext>(MockBehavior.Strict);
            entityContext.SetupGet(instance => instance.Mappings).Returns(mappings.Object);
            entityContext.Setup(instance => instance.Create<IClass>(It.IsAny<Iri>())).Returns<Iri>(id => CreateClass(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<ISupportedProperty>(It.IsAny<Iri>())).Returns<Iri>(id => CreateSupportedProperty(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<IProperty>(It.IsAny<Iri>())).Returns<Iri>(id => CreateProperty<IProperty>(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<IInverseFunctionalProperty>(It.IsAny<Iri>())).Returns<Iri>(id => CreateProperty<IInverseFunctionalProperty>(entityContext.Object, id));
            entityContext.Setup(instance => instance.Create<IRestriction>(It.IsAny<Iri>())).Returns<Iri>(id => CreateRestriction(entityContext.Object, id));
            var result = new Mock<IApiDocumentation>(MockBehavior.Strict);
            result.SetupGet(instance => instance.Iri).Returns(new Iri("http://temp.uri/api"));
            result.SetupGet(instance => instance.Context).Returns(entityContext.Object);
            return result.Object;
        }

        private static void SetupEntityMapping<T>(Mock<IMappingsRepository> mappingsRepository, Iri term, IDictionary<string, Iri> properties = null) where T : IEntity
        {
            var statmentMappings = new List<IStatementMapping>();
            var statementMapping = new Mock<IStatementMapping>(MockBehavior.Strict);
            statementMapping.SetupGet(instance => instance.Term).Returns(term);
            statmentMappings.Add(statementMapping.Object);

            var propertyMappings = new List<IPropertyMapping>();
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    var propertyMapping = new Mock<IPropertyMapping>(MockBehavior.Strict);
                    propertyMapping.SetupGet(instance => instance.Term).Returns(property.Value);
                    propertyMapping.SetupGet(instance => instance.Name).Returns(property.Key);
                    propertyMappings.Add(propertyMapping.Object);
                }
            }

            var entityMapping = new Mock<IEntityMapping>(MockBehavior.Strict);
            entityMapping.SetupGet(instance => instance.Type).Returns(typeof(IClass));
            entityMapping.SetupGet(instance => instance.Classes).Returns(statmentMappings);
            entityMapping.SetupGet(instance => instance.Properties).Returns(propertyMappings);
            mappingsRepository.Setup(instance => instance.FindEntityMappingFor<T>()).Returns(entityMapping.Object);
            mappingsRepository.Setup(instance => instance.FindEntityMappingFor(typeof(T))).Returns(entityMapping.Object);
        }

        private static IResource CreateResource(IEntityContext entityContext, Iri id)
        {
            var result = new Mock<IResource>(MockBehavior.Strict);
            string label = null;
            string description = null;
            result.SetupSet(instance => instance.Label = It.IsAny<string>()).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = It.IsAny<string>()).Callback<string>(value => description = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Iri).Returns(id);
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupGet(instance => instance.Description).Returns(() => description);
            return result.Object;
        }

        private static IClass CreateClass(IEntityContext entityContext, Iri id)
        {
            var entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var result = entityMock.As<IClass>();
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(new[] { typeof(IClass) });
            string label = null;
            string description = null;
            result.SetupSet(instance => instance.Label = It.IsAny<string>()).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = It.IsAny<string>()).Callback<string>(value => description = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Iri).Returns(id);
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupGet(instance => instance.Description).Returns(() => description);
            result.SetupGet(instance => instance.SupportedProperties).Returns(new List<ISupportedProperty>());
            result.SetupGet(instance => instance.SubClassOf).Returns(new List<URSA.Web.Http.Description.Rdfs.IClass>());
            return result.Object;
        }

        private static ISupportedProperty CreateSupportedProperty(IEntityContext entityContext, Iri id)
        {
            var propertyFullName = id.ToString().Substring(id.ToString().LastIndexOf(':') + 1);
            var declaringTypeName = propertyFullName.Substring(0, propertyFullName.LastIndexOf('.'));
            var propertyName = propertyFullName.Substring(declaringTypeName.Length + 1);
            var propertyInfo = Type.GetType(declaringTypeName).GetProperty(propertyName);
            var readOnly = propertyInfo.CanRead;
            var writeOnly = propertyInfo.CanWrite;
            var required = (propertyInfo.PropertyType.GetTypeInfo().IsValueType) || (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null);
            var result = new Mock<ISupportedProperty>(MockBehavior.Strict);
            IProperty property = null;
            result.SetupSet(instance => instance.Readable = It.Is<bool>(value => value == readOnly));
            result.SetupSet(instance => instance.Writeable = It.Is<bool>(value => value == writeOnly));
            result.SetupSet(instance => instance.Required = It.Is<bool>(value => value == required));
            result.SetupSet(instance => instance.Property = It.Is<IProperty>(value => value.Iri.ToString().Contains(propertyFullName))).Callback<IProperty>(value => property = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Iri).Returns(id);
            result.SetupGet(instance => instance.Readable).Returns(readOnly);
            result.SetupGet(instance => instance.Writeable).Returns(writeOnly);
            result.SetupGet(instance => instance.Required).Returns(required);
            result.SetupGet(instance => instance.Property).Returns(() => property);
            return result.Object;
        }

        private static TProperty CreateProperty<TProperty>(IEntityContext entityContext, Iri id) where TProperty : class, IProperty
        {
            string propertyFullName = (id == EntityConverter.Hydra.AbsoluteUri + "member" ? 
                String.Format("{0}.Members", typeof(ICollection).FullName) : 
                id.ToString().Substring(id.ToString().LastIndexOf(':') + 1));
            var declaringTypeName = propertyFullName.Substring(0, propertyFullName.LastIndexOf('.'));
            var propertyName = propertyFullName.Substring(declaringTypeName.Length + 1);
            var propertyInfo = (propertyName == "Members" ? typeof(ICollection) : Type.GetType(declaringTypeName)).GetProperty(propertyName);
            var result = new Mock<TProperty>(MockBehavior.Strict);
            string label = null;
            string description = null;
            result.SetupSet(instance => instance.Label = propertyInfo.Name).Callback<string>(value => label = value);
            result.SetupSet(instance => instance.Description = propertyFullName).Callback<string>(value => description = value);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Iri).Returns(id);
            result.SetupGet(instance => instance.Label).Returns(() => label);
            result.SetupGet(instance => instance.Description).Returns(() => description);
            result.SetupGet(instance => instance.Domain).Returns(new List<URSA.Web.Http.Description.Rdfs.IClass>());
            result.SetupGet(instance => instance.Range).Returns(new List<URSA.Web.Http.Description.Rdfs.IResource>());
            return result.Object;
        }

        private static IRestriction CreateRestriction(IEntityContext entityContext, Iri id)
        {
            var entityMock = new Mock<MulticastObject>(MockBehavior.Strict);
            var result = entityMock.As<IRestriction>();
            entityMock.SetupGet(instance => instance.CastedTypes).Returns(new[] { typeof(IRestriction) });
            IProperty property = null;
            IEntity allValuesFrom = null;
            uint maxCardinality = 0;
            result.SetupSet(instance => instance.OnProperty = It.IsAny<IProperty>()).Callback<IProperty>(value => property = value);
            result.SetupSet(instance => instance.MaxCardinality = It.IsAny<uint>()).Callback<uint>(value => maxCardinality = value);
            result.SetupSet(instance => instance.AllValuesFrom = It.IsAny<IEntity>()).Callback<IEntity>(value => allValuesFrom = value);
            result.SetupGet(instance => instance.AllValuesFrom).Returns(() => allValuesFrom);
            result.SetupGet(instance => instance.Context).Returns(entityContext);
            result.SetupGet(instance => instance.Iri).Returns(id);
            result.SetupGet(instance => instance.MaxCardinality).Returns(() => maxCardinality);
            result.SetupGet(instance => instance.OnProperty).Returns(() => property);
            result.SetupGet(instance => instance.SubClassOf).Returns(new List<URSA.Web.Http.Description.Rdfs.IClass>());
            return result.Object;
        }
    }
}