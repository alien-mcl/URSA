using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomanticWeb;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a description context.</summary>
    public class DescriptionContext
    {
        private readonly IDictionary<Type, Tuple<IClass, bool>> _typeDefinitions;

        /// <summary>Initializes a new instance of the <see cref="DescriptionContext"/> class.</summary>
        /// <param name="apiDocumentation">The API documentation.</param>
        /// <param name="typeDescriptionBuilder">Type description builder.</param>
        /// <param name="type">The type.</param>
        /// <param name="typeDefinitions">The type definitions.</param>
        [ExcludeFromCodeCoverage]
        private DescriptionContext(IApiDocumentation apiDocumentation, ITypeDescriptionBuilder typeDescriptionBuilder, Type type, IDictionary<Type, Tuple<IClass, bool>> typeDefinitions)
        {
            if (apiDocumentation == null)
            {
                throw new ArgumentNullException("apiDocumentation");
            }

            if (typeDescriptionBuilder == null)
            {
                throw new ArgumentNullException("typeDescriptionBuilder");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (typeDefinitions == null)
            {
                throw new ArgumentNullException("typeDefinitions");
            }

            ApiDocumentation = apiDocumentation;
            TypeDescriptionBuilder = typeDescriptionBuilder;
            Type = type;
            _typeDefinitions = typeDefinitions;
        }

        /// <summary>Gets the API documentation.</summary>
        public IApiDocumentation ApiDocumentation { get; private set; }

        /// <summary>Gets the type description builder.</summary>
        public ITypeDescriptionBuilder TypeDescriptionBuilder { get; private set; }

        /// <summary>Gets the type.</summary>
        public Type Type { get; private set; }

        /// <summary>Gets the description for given <paramref name="type" />.</summary>
        /// <param name="type">The type for which to obtain the description.</param>
        /// <returns>Instance of the <see cref="IResource" /> containing the description of given <paramref name="type" />.</returns>
        public IClass this[Type type] { get { return _typeDefinitions[type].Item1; } }

        /// <summary>Creates a copy of the context for different type.</summary>
        /// <param name="apiDocumentation">The API documentation.</param>
        /// <param name="type">The type.</param>
        /// <param name="typeDescriptionBuilder">Type description builder.</param>
        /// <returns>Instance of the <see cref="DescriptionContext" /> for given <paramref name="type" />.</returns>
        [ExcludeFromCodeCoverage]
        public static DescriptionContext ForType(IApiDocumentation apiDocumentation, Type type, ITypeDescriptionBuilder typeDescriptionBuilder)
        {
            if (apiDocumentation == null)
            {
                throw new ArgumentNullException("apiDocumentation");
            }

            if (typeDescriptionBuilder == null)
            {
                throw new ArgumentNullException("typeDescriptionBuilder");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return new DescriptionContext(apiDocumentation, typeDescriptionBuilder, type, new Dictionary<Type, Tuple<IClass, bool>>());
        }

        /// <summary>Creates a copy of the context for different type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>Instance of the <see cref="DescriptionContext" /> for given <paramref name="type" />.</returns>
        [ExcludeFromCodeCoverage]
        public DescriptionContext ForType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return (type == Type ? this : new DescriptionContext(ApiDocumentation, TypeDescriptionBuilder, type, _typeDefinitions));
        }

        /// <summary>Builds the type description.</summary>
        /// <returns>Description of the type.</returns>
        public IClass BuildTypeDescription()
        {
            return TypeDescriptionBuilder.BuildTypeDescription(this);
        }

        /// <summary> Determines whether the given <paramref name="type" /> is already described and available in the context.</summary>
        /// <param name="type">The type to check for existence.</param>
        /// <returns><b>true</b> if the type is already available in the context; otherwise <b>false</b>.</returns>
        public bool ContainsType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return _typeDefinitions.ContainsKey(type);
        }

        /// <summary>Checks whether the given <paramref name="type"/> requires an RDF approach.</summary>
        /// <param name="type">The type to check.</param>
        /// <returns><b>true</b> if the type requires RDF approach; otherwise <b>false</b>.</returns>
        public bool RequiresRdf(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return _typeDefinitions[type].Item2;
        }

        /// <summary>Describes the current type.</summary>
        /// <param name="resource">The resource describing a type.</param>
        /// <param name="requiresRdf">Flag determining whether the type requires RDF approach.</param>
        public void Describe(IClass resource, bool requiresRdf)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            _typeDefinitions[Type] = new Tuple<IClass, bool>(resource, requiresRdf);
        }
    }
}