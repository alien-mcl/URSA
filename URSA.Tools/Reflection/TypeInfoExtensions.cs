using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    /// <summary>Provides useful <see cref="Type" /> extensions.</summary>
    public static class TypeInfoExtensions
    {
        /// <summary>Checks if a given type is of either type <see cref="IList" /> or <see cref="IList{T}"/>.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsList(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var listTypeInfo = typeof(IList).GetTypeInfo();
            return (!type.IsArray) && ((type.ImplementedInterfaces.Any(@interface => (listTypeInfo.IsAssignableFrom(@interface)))) || (type.IsGenericList()));
        }

        /// <summary>Checks if a given type is of type <see cref="IList{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsGenericList(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var listTypeInfo = typeof(IList<>).GetTypeInfo();
            return (!type.IsArray) && (((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(IList<>))) ||
                (type.ImplementedInterfaces.Any(@interface => (@interface.GetTypeInfo().IsGenericType) && (listTypeInfo.IsAssignableFrom(@interface.GetGenericTypeDefinition())))));
        }

        /// <summary>Checks if a given type is of either type <see cref="ICollection" /> or <see cref="ICollection{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsCollection(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var collectionTypeInfo = typeof(ICollection).GetTypeInfo();
            return (!type.IsArray) && ((type.ImplementedInterfaces.Any(@interface => (collectionTypeInfo.IsAssignableFrom(@interface)))) || (type.IsGenericCollection()));
        }

        /// <summary>Checks if a given type is of type <see cref="ICollection{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsGenericCollection(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var collectionTypeInfo = typeof(ICollection<>).GetTypeInfo();
            return (!type.IsArray) && (((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(ICollection<>))) ||
                (type.ImplementedInterfaces.Any(@interface => (@interface.GetTypeInfo().IsGenericType) && (collectionTypeInfo.IsAssignableFrom(@interface.GetGenericTypeDefinition())))));
        }

        /// <summary>Checks if the type can be assigned to the <see cref="IEnumerable" /> interface.</summary> 
        /// <remarks>This method will return false for type <see cref="System.String" />.</remarks> 
        /// <param name="type">Type to be checked.</param> 
        /// <returns><b>true</b> if the type is <see cref="System.Array" /> or is assignable to <see cref="IEnumerable" /> (except <see cref="System.String" />); otherwise <b>false</b>.</returns> 
        public static bool IsEnumerable(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return ((type.IsArray) || ((typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type)) && (type.AsType() != typeof(string)))) || (type.IsGenericEnumerable());
        } 

        /// <summary>Checks if a given type is of type <see cref="IEnumerable{T}" />.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><b>true</b> if the type implements a proper interface; otherwise <b>false</b>.</returns>
        public static bool IsGenericEnumerable(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var enumerableTypeInfo = typeof(IEnumerable<>).GetTypeInfo();
            return (!type.IsArray) && (type.AsType() != typeof(string)) && (((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))) ||
                (type.ImplementedInterfaces.Any(@interface => (@interface.GetTypeInfo().IsGenericType) && (enumerableTypeInfo.IsAssignableFrom(@interface.GetGenericTypeDefinition())))));
        }

        /// <summary>Gets the item type of the collection type or the type itself.</summary>
        /// <param name="type">Type for which find the item type.</param>
        /// <returns><see cref="Type" /> being the item of the collection or the <paramref name="type" /> itself.</returns>
        public static Type GetItemType(this TypeInfo type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if ((type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return type.GetGenericArguments().First();
            }
            
            if ((typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type)) && (type.AsType() != typeof(string)))
            {
                return (type.IsGenericType ? type.GenericTypeArguments.First() : typeof(object));
            }

            return type.AsType();
        }

        /// <summary>Creates an instance of the <paramref name="targetType" /> from an enumeration of objects.</summary>
        /// <param name="values">Objects to feed the new instance.</param>
        /// <param name="targetType">Target type of the instance.</param>
        /// <param name="itemType">Optional collection item type.</param>
        /// <returns>Instance of the <paramref name="targetType" />.</returns>
        public static object MakeInstance(this IEnumerable<object> values, TypeInfo targetType, Type itemType = null)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (values == null)
            {
                return null;
            }

            if (!targetType.IsEnumerable())
            {
                return ((targetType.IsGenericType) && (targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) ?
                    typeof(Nullable<>).MakeGenericType(itemType).GetTypeInfo().GetConstructor(new[] { itemType }).Invoke(new[] { Convert.ChangeType(values.First(), itemType) }) :
                    Convert.ChangeType(values.First(), targetType.AsType()));
            }

            return CreateEnumerable(values, targetType, itemType ?? typeof(object));
        }

        /// <summary>Gets the first encountered actual type definition of any generic <paramref name="implementations"/> provided.</summary>
        /// <param name="type">The type to check for implementation.</param>
        /// <param name="implementations">The implementations to be searched for.</param>
        /// <returns>First encountered type definition matching any of the <paramref name="implementations" /> provided.</returns>
        public static Type GetImplementationOfAny(this TypeInfo type, params Type[] implementations)
        {
            return (from implementation in implementations
                    from @interface in type.ImplementedInterfaces
                    where (@interface.GetTypeInfo().IsGenericType) && (implementation == @interface.GetGenericTypeDefinition())
                    select @interface).FirstOrDefault();
        }

        private static IEnumerable CreateEnumerable(IEnumerable<object> values, TypeInfo type, Type itemType)
        {
            if (values.GetType() == type.AsType())
            {
                return values;
            }

            if (type.IsArray)
            {
                int count = values.Count();
                Array result = Array.CreateInstance(itemType, count);
                int index = 0;
                foreach (var item in values)
                {
                    result.SetValue(Convert.ChangeType(item, itemType), index);
                    index++;
                }

                return result;
            }
            else
            {
                IList result = (IList)typeof(IList<>).MakeGenericType(itemType).GetTypeInfo().GetConstructor(new Type[0]).Invoke(null);
                foreach (var item in values)
                {
                    result.Add(Convert.ChangeType(item, itemType));
                }

                return result;
            }
        }
    }
}