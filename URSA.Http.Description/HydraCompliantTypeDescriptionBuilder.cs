using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using URSA.Reflection;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using IClass = URSA.Web.Http.Description.Hydra.IClass;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a type description building facility.</summary>
    public class HydraCompliantTypeDescriptionBuilder : ITypeDescriptionBuilder
    {
        private static readonly IDictionary<Type, Uri> TypeDescriptions = XsdUriParser.Types.Concat(OGuidUriParser.Types).ToDictionary(item => item.Key, item => item.Value);

        private static readonly Uri[] SupportedMediaTypeProfiles = { EntityConverter.Hydra };

        private readonly IXmlDocProvider _xmlDocProvider;

        /// <summary>Initializes a new instance of the <see cref="HydraCompliantTypeDescriptionBuilder"/> class.</summary>
        /// <param name="xmlDocProvider">The XML documentation provider.</param>
        public HydraCompliantTypeDescriptionBuilder(IXmlDocProvider xmlDocProvider)
        {
            if (xmlDocProvider == null)
            {
                throw new ArgumentNullException("xmlDocProvider");
            }

            _xmlDocProvider = xmlDocProvider;
        }

        /// <inheritdoc />
        public IEnumerable<Uri> SupportedProfiles { get { return SupportedMediaTypeProfiles; } }

        /// <inheritdoc />
        public IResource BuildTypeDescription(DescriptionContext context)
        {
            bool requiresRdf;
            return BuildTypeDescription(context, out requiresRdf);
        }

        /// <inheritdoc />
        public IResource BuildTypeDescription(DescriptionContext context, out bool requiresRdf)
        {
            requiresRdf = false;
            Type itemType = context.Type.GetItemType();
            if (TypeDescriptions.ContainsKey(itemType))
            {
                return BuildDatatypeDescription(context, TypeDescriptions[itemType]);
            }

            if (context.Type.IsList())
            {
                return CreateListDefinition(context, out requiresRdf, context.Type.IsGenericList());
            }

            var classUri = itemType.MakeUri();
            if (typeof(IEntity).IsAssignableFrom(itemType))
            {
                classUri = context.ApiDocumentation.Context.Mappings.MappingFor(itemType).Classes.Select(item => item.Uri).FirstOrDefault() ?? classUri;
                requiresRdf = true;
            }

            IClass @class;
            var result = context.ApiDocumentation.Context.Create<IResource>(context.ApiDocumentation.CreateBlankId());
            result.SingleValue = !System.Reflection.TypeExtensions.IsEnumerable(context.Type);
            result.Type = @class = context.ApiDocumentation.Context.Create<IClass>(classUri);
            @class.Label = itemType.Name.Replace("&", String.Empty);
            @class.Description = _xmlDocProvider.GetDescription(itemType);
            if (typeof(EntityId).IsAssignableFrom(itemType))
            {
                context.Describe(result, requiresRdf);
                return result;
            }

            SetupProperties(context.ForType(itemType), @class);
            context.Describe(result, requiresRdf);
            return result;
        }

        private static IResource BuildDatatypeDescription(DescriptionContext context, Uri uri)
        {
            var definition = context.ApiDocumentation.Context.Create<IResource>(context.ApiDocumentation.CreateBlankId());
            definition.SingleValue = !System.Reflection.TypeExtensions.IsEnumerable(context.Type);
            definition.Type = context.ApiDocumentation.Context.Create<IResource>(new EntityId(uri));
            definition.Type.Label = (uri.Fragment.Length > 1 ? uri.Fragment.Substring(1) : uri.Segments.Last());
            return definition;
        }

        private void SetupProperties(DescriptionContext context, IClass @class)
        {
            IResource existingType;
            if ((context.ContainsType(context.Type)) &&
                (context[context.Type].Is(context.ApiDocumentation.Context.Mappings.MappingFor<IResource>().Classes.First().Uri)) &&
                ((existingType = context[context.Type].Type) != null) &&
                (existingType.Is(context.ApiDocumentation.Context.Mappings.MappingFor<IClass>().Classes.First().Uri)))
            {
                foreach (var property in existingType.AsEntity<IClass>().SupportedProperties)
                {
                    @class.SupportedProperties.Add(property);
                }
            }
            else
            {
                foreach (var property in context.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    @class.SupportedProperties.Add(BuildSupportedProperty(context, @class, property));
                }
            }
        }

        private IResource CreateListDefinition(DescriptionContext context, out bool requiresRdf, bool isGeneric = true)
        {
            requiresRdf = false;
            IResource result = context.ApiDocumentation.Context.Create<IResource>(context.ApiDocumentation.CreateBlankId());
            if (!isGeneric)
            {
                result.Type = context.ApiDocumentation.Context.Create<IClass>(new EntityId(Rdf.List));
                return result;
            }

            IClass @class;
            var itemType = context.Type.GetItemType();
            result.Type = @class = context.ApiDocumentation.Context.Create<IClass>(new EntityId(context.Type.MakeUri()));
            @class.Label = context.Type.Name;
            @class.Description = _xmlDocProvider.GetDescription(context.Type);
            @class.SubClassOf.Add(context.ApiDocumentation.Context.Create<IClass>(new EntityId(Rdf.List)));

            @class.SubClassOf.Add(result.CreateRestriction(Rdf.first, (context.ContainsType(itemType) ? context[itemType] : BuildTypeDescription(context.ForType(itemType), out requiresRdf)).Type));
            requiresRdf |= context.RequiresRdf(itemType);

            @class.SubClassOf.Add(result.CreateRestriction(Rdf.rest, result.Type));
            context.Describe(result, requiresRdf);
            return result;
        }

        private ISupportedProperty BuildSupportedProperty(DescriptionContext context, IClass @class, PropertyInfo property)
        {
            var propertyId = new EntityId(property.MakeUri());
            var propertyUri = (!typeof(IEntity).IsAssignableFrom(context.Type) ? @class.Id.Uri.AddName(property.Name) :
                context.ApiDocumentation.Context.Mappings.MappingFor(context.Type).PropertyFor(property.Name).Uri);
            var result = context.ApiDocumentation.Context.Create<ISupportedProperty>(propertyId);
            result.ReadOnly = (!property.PropertyType.IsCollection()) && (!property.CanWrite);
            result.WriteOnly = !property.CanRead;
            result.Required = (property.PropertyType.IsValueType) || (property.GetCustomAttribute<RequiredAttribute>() != null);
            result.Property = (property.GetCustomAttribute<KeyAttribute>() != null ?
                context.ApiDocumentation.Context.Create<IInverseFunctionalProperty>(propertyUri) :
                context.ApiDocumentation.Context.Create<IProperty>(propertyUri));
            result.Property.Label = property.Name;
            result.Property.Description = _xmlDocProvider.GetDescription(property);
            result.Property.Domain.Add(@class);
            if (!context.ContainsType(property.PropertyType))
            {
                bool requiresRdf;
                var childContext = context.ForType(property.PropertyType);
                var propertyType = BuildTypeDescription(childContext, out requiresRdf);
                childContext.Describe(propertyType, requiresRdf);
            }

            result.Property.Range.Add(context[property.PropertyType].Type);
            if ((System.Reflection.TypeExtensions.IsEnumerable(property.PropertyType)) && (property.PropertyType != typeof(byte[])))
            {
                return result;
            }

            @class.SubClassOf.Add(@class.CreateRestriction(result.Property, 1u));
            return result;
        }
    }
}