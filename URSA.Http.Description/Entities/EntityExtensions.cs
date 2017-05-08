using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RDeF.Entities;
using RDeF.Mapping;
using RollerCaster;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using URSA.Web.Http.Description.Reflection;
using ICollection = URSA.Web.Http.Description.Hydra.ICollection;
using IResource = URSA.Web.Http.Description.Hydra.IResource;

namespace URSA.Web.Http.Description.Entities
{
    /// <summary>Provides useful <see cref="IEntity" /> extensions.</summary>
    public static class EntityExtensions
    {
        internal static readonly MethodInfo ActLikeMethod = typeof(DynamicExtensions).GetMethods().First(method => method.Name == "ActLike" && method.IsGenericMethod);
        private static readonly MethodInfo CopyT = typeof(DefaultEntityContext).GetMethod("Copy");

        /// <summary>Updates the specified <paramref name="target" /> entity with values from <paramref name="source" /> entity.</summary>
        /// <remarks>This method won't update nested entities' properties if the source values are <b>null</b>.</remarks>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="target">Target entity to be updated.</param>
        /// <param name="source">Source entity to get values from.</param>
        /// <returns>Updated <paramref name="target" /> entity.</returns>
        public static T Update<T>(this T target, T source) where T : class, IEntity
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return target.Update(source, new HashSet<Iri>(),  0);
        }

        internal static bool IsCollection(this Rdfs.IClass @class)
        {
            return @class.IsClass(@class.Context.Mappings.FindEntityMappingFor<ICollection>().Classes.First().Term);
        }

        internal static IRestriction CreateRestriction(this IResource resource, Uri onProperty, IEntity allValuesFrom)
        {
            var typeConstrain = resource.Context.Create<IRestriction>(new Iri());
            typeConstrain.OnProperty = resource.Context.Create<IProperty>(new Iri(onProperty));
            typeConstrain.AllValuesFrom = allValuesFrom;
            return typeConstrain;
        }

        internal static IRestriction CreateRestriction(this IResource resource, IProperty onProperty, uint maxCardinality)
        {
            var typeConstrain = resource.Context.Create<IRestriction>(new Iri());
            typeConstrain.OnProperty = onProperty;
            typeConstrain.MaxCardinality = maxCardinality;
            return typeConstrain;
        }

        internal static bool IsClass(this Rdfs.IClass @class, Iri type)
        {
            return ((!(@class.Iri.IsBlank)) && (@class.Iri == type)) ||
                (@class.SubClassOf.Any(superClass => superClass.IsClass(type)));
        }

        internal static IEnumerable<Rdfs.IClass> SuperClasses(this Rdfs.IClass @class)
        {
            IList<Rdfs.IClass> result = new List<Rdfs.IClass>();
            foreach (var superClass in @class.SubClassOf)
            {
                result.AddRange(superClass.SuperClasses());
                result.Add(superClass);
            }

            return result;
        }

        private static object Copy(this IEntityContext entityContext, Type type, IEntity entity)
        {
            return CopyT.MakeGenericMethod(type).Invoke(entityContext, new object[] { entity, null });
        }

        private static T Update<T>(this T targetEntity, T sourceEntity, ISet<Iri> visited, int depth) where T : class, IEntity
        {
            if (targetEntity == null)
            {
                return null;
            }

            if ((sourceEntity == null) && (depth > 0))
            {
                return targetEntity;
            }

            if (targetEntity == sourceEntity)
            {
                return targetEntity;
            }

            var target = targetEntity.Unwrap();
            var source = sourceEntity.Unwrap();
            foreach (var property in targetEntity.Context.Mappings.FindEntityMappingFor<T>().Properties)
            {
                var value = source.GetProperty(typeof(T), property.Name);
                if (value == null)
                {
                    if (depth == 0)
                    {
                        target.SetProperty(typeof(T), property.Name, null);
                    }

                    continue;
                }

                if (property.ReturnType.GetTypeInfo().IsEnumerable())
                {
                    value = targetEntity.UpdateCollection(property, (IEnumerable)value, visited, depth);
                }
                else if (value is IEntity)
                {
                    var current = (IEntity)target.GetProperty(typeof(T), property.Name);
                    value = (current != null ? current.Update((IEntity)value) :
                        ActLikeMethod.MakeGenericMethod(property.ReturnType).Invoke(null, new[] { targetEntity.Context.Copy((IEntity)value) })); 
                }

                if (value != null)
                {
                    target.SetProperty(typeof(T), property.Name, value);
                }
            }

            return targetEntity;
        }

        private static object UpdateCollection<T>(this T targetEntity, IPropertyMapping property, IEnumerable sourceValues, ISet<Iri> visited, int depth = 0) where T : class, IEntity
        {
            var target = targetEntity.Unwrap();
            int indexOf = -1;
            IEnumerable current = (IEnumerable)target.GetProperty(typeof(T), property.Name);
            var collection = (current != null ? new AbstractCollectionWrapper(current) : new AbstractCollectionWrapper(new List<object>()) { IsReplaced = true });
            var itemType = property.ReturnType.GetTypeInfo().GetItemType();
            foreach (var item in sourceValues)
            {
                var itemEntity = item as IEntity;
                var existing = collection
                    .Where((element, index) => (itemEntity != null ? itemEntity.Iri == ((IEntity)element).Iri : Equals(element, item)) && ((indexOf = index) != -1))
                    .FirstOrDefault();
                if (existing == null)
                {
                    collection.Add(itemEntity != null ? targetEntity.Context.Copy(itemType, itemEntity) : item);
                }
                else if (itemEntity != null)
                {
                    ((IEntity)existing).Update(itemEntity, visited, depth + 1);
                }
                else if (collection.IsList)
                {
                    collection[indexOf] = item;
                }
                else
                {
                    collection.Remove(existing);
                    collection.Add(item);
                }
            }

            var sourceCollection = new AbstractCollectionWrapper(sourceValues);
            var toBeRemoved = (from object item in current 
                               let existing = sourceCollection.Where((element, index) => (item is IEntity ? ((IEntity)item).Iri == ((IEntity)element).Iri : Equals(element, item)) && ((indexOf = index) != -1)).FirstOrDefault() 
                               where existing == null 
                               select item).ToList();
            foreach (var item in toBeRemoved)
            {
                collection.Remove(item);
            }

            return (collection.IsReplaced ? collection.Collection : null);
        }
    }
}