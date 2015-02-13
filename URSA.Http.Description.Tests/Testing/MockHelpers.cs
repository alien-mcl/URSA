using Moq;
using RomanticWeb;
using RomanticWeb.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace URSA.Web.Http.Description.Testing
{
    /// <summary>Provides Moq helper methods.</summary>
    public static class MockHelpers
    {
        /// <summary>Creates an <see cref="IEntity" /> mock.</summary>
        /// <typeparam name="T">Type of the entity to mock.</typeparam>
        /// <param name="context">Context to pass to the entity.</param>
        /// <param name="id">Identifier of the entity.</param>
        /// <returns>Mock of the type of the <typeparamref name="T" /> entity.</returns>
        public static Mock<T> MockEntity<T>(IEntityContext context, EntityId id) where T : class, IEntity
        {
            Mock<T> result = new Mock<T>() { DefaultValue = DefaultValue.Mock };
            result.SetupGet(instance => instance.Id).Returns(id);
            result.SetupGet(instance => instance.Context).Returns(context);
            var collections = from @interface in new Type[] { typeof(T) }.Concat(typeof(T).GetInterfaces())
                              from property in @interface.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              where (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) &&
                                (property.PropertyType != typeof(string))
                              select property;
            foreach (var property in collections)
            {
                var parameter = Expression.Parameter(property.DeclaringType, "instance");
                var expression = Expression.Lambda(Expression.MakeMemberAccess(parameter, property), parameter);
                IEnumerable collection = (IEnumerable)typeof(List<>).MakeGenericType(property.PropertyType.GetItemType()).GetConstructor(new Type[0]).Invoke(null);
                Mock mock = result;
                if (property.DeclaringType != typeof(T))
                {
                    mock = (Mock)mock.GetType().GetMethod("As", BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(property.DeclaringType).Invoke(mock, null);
                }

                object getterSetup = mock.GetType().GetMethod("SetupGet", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(property.PropertyType)
                    .Invoke(mock, new object[] { expression });
                var getterSetupType = typeof(Moq.Language.IReturnsGetter<,>)
                    .MakeGenericType(property.DeclaringType, property.PropertyType);
                getterSetup.GetType().GetInterfaceMap(getterSetupType)
                    .TargetMethods
                    .First(method => (method.Name == "Returns") && (method.GetParameters().Length == 1) &&
                        (method.GetParameters()[0].ParameterType == property.PropertyType))
                    .Invoke(getterSetup, new object[] { collection });
            }

            return result;
        }
    }
}