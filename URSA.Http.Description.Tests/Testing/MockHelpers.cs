using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using RDeF.Entities;
using RDeF.Mapping;
using RollerCaster;

namespace URSA.Web.Http.Description.Testing
{
    /// <summary>Provides Moq helper methods.</summary>
    [ExcludeFromCodeCoverage]
    public static class MockHelpers
    {
        /// <summary>Creates an <see cref="IEntity" /> mock.</summary>
        /// <typeparam name="T">Type of the entity to mock.</typeparam>
        /// <param name="context">Mock of an entity context to pass to the entity.</param>
        /// <param name="id">Identifier of the entity.</param>
        /// <returns>Mock of the type of the <typeparamref name="T" /> entity.</returns>
        public static Mock<T> MockEntity<T>(this Mock<IEntityContext> context, Iri id) where T : class, IEntity
        {
            Mock<MulticastObject> entityMock;
            return context.MockEntity<T>(id, out entityMock);
        }

        /// <summary>Creates an <see cref="IEntity" /> mock.</summary>
        /// <typeparam name="T">Type of the entity to mock.</typeparam>
        /// <param name="context">Mock of an entity context to pass to the entity.</param>
        /// <param name="id">Identifier of the entity.</param>
        /// <param name="entityMock">Mock of the orignal entity.</param>
        /// <returns>Mock of the type of the <typeparamref name="T" /> entity.</returns>
        public static Mock<T> MockEntity<T>(this Mock<IEntityContext> context, Iri id, out Mock<MulticastObject> entityMock) where T : class, IEntity
        {
            entityMock = new Mock<MulticastObject>() { DefaultValue = DefaultValue.Mock };
            var result = entityMock.As<T>();
            result.SetupGet(instance => instance.Iri).Returns(id);
            result.SetupGet(instance => instance.Context).Returns(context.Object);
            var collections = from @interface in new Type[] { typeof(T) }.Concat(typeof(T).GetInterfaces())
                              from property in @interface.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              where (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) &&
                                (property.PropertyType != typeof(string))
                              select property;
            foreach (var property in collections)
            {
                var parameter = Expression.Parameter(property.DeclaringType, "instance");
                var expression = Expression.Lambda(Expression.MakeMemberAccess(parameter, property), parameter);
                IEnumerable collection = (IEnumerable)typeof(List<>).MakeGenericType(property.PropertyType.GetTypeInfo().GetItemType()).GetConstructor(new Type[0]).Invoke(null);
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
                getterSetup.GetType().GetTypeInfo().GetRuntimeInterfaceMap(getterSetupType)
                    .TargetMethods
                    .First(method => (method.Name == "Returns") && (method.GetParameters().Length == 1) &&
                        (method.GetParameters()[0].ParameterType == property.PropertyType))
                    .Invoke(getterSetup, new object[] { collection });
            }

            return result;
        }

        /// <summary>Setups the class mapping.</summary>
        /// <typeparam name="T">Type of the class to map.</typeparam>
        /// <param name="mappingsRepository">The mock of a mappings repository.</param>
        /// <param name="baseUri">The base URI.</param>
        public static void SetupMapping<T>(this Mock<IMappingsRepository> mappingsRepository, Uri baseUri) where T : IEntity
        {
            var classMapping = new Mock<IStatementMapping>(MockBehavior.Strict);
            classMapping.SetupGet(instance => instance.Term).Returns(new Uri(baseUri.AbsoluteUri + typeof(T).Name.Substring(1)));
            var mapping = new Mock<IEntityMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Classes).Returns(new[] { classMapping.Object });
            mappingsRepository.Setup(instance => instance.FindEntityMappingFor<T>()).Returns(mapping.Object);
            mappingsRepository.Setup(instance => instance.FindEntityMappingFor(typeof(T))).Returns(mapping.Object);
        }
    }
}