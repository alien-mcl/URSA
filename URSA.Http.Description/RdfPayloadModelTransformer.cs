using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RomanticWeb.Entities;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Copies <see cref="IEntity" /> based responses to the in-memory RDF graph that will be used as an actual request/response payload.</summary>
    public class RdfPayloadModelTransformer : IResponseModelTransformer, IRequestModelTransformer
    {
        private readonly IEntityContextProvider _entityContextProvider;

        /// <summary>Initializes a new instance of the <see cref="RdfPayloadModelTransformer"/> class.</summary>
        /// <param name="entityContextProvider">The entity context provider.</param>
        public RdfPayloadModelTransformer(IEntityContextProvider entityContextProvider)
        {
            if (entityContextProvider == null)
            {
                throw new ArgumentNullException("entityContextProvider");
            }

            _entityContextProvider = entityContextProvider;
        }

        /// <inheritdoc />
        public async Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
        {
            return await Transform(result);
        }

        /// <inheritdoc />
        public async Task<object[]> Transform(object[] arguments)
        {
            for (int index = 0; index < arguments.Length; index++)
            {
                arguments[index] = await Transform(arguments[index]);
            }

            return arguments;
        }

        private Task<object> Transform(object argument)
        {
            Type itemType;
            if ((argument == null) || (!typeof(IEntity).GetTypeInfo().IsAssignableFrom(itemType = argument.GetType().FindItemType())))
            {
                return Task.FromResult(argument);
            }

            if (argument.GetType().IsEnumerable())
            {
                return TransformCollection((IEnumerable<IEntity>)argument, itemType);
            }

            IEntity entity = (IEntity)argument;
            if (entity.Context == null)
            {
                return Task.FromResult(argument);
            }

            entity = _entityContextProvider.EntityContext.Copy(entity);
            _entityContextProvider.EntityContext.Commit();
            return Task.FromResult((object)entity);
        }

        private Task<object> TransformCollection(IEnumerable<IEntity> collection, Type itemType)
        {
            IEntity entity = collection.FirstOrDefault();
            if (entity == null)
            {
                return Task.FromResult((object)Array.CreateInstance(itemType, 0));
            }

            if (entity.Context == null)
            {
                return Task.FromResult((object)collection);
            }

            var output = (IList)typeof(List<>).MakeGenericType(itemType).GetConstructor(new Type[0]).Invoke(null);
            foreach (var item in collection)
            {
                var copy = _entityContextProvider.EntityContext.Copy(item);
                output.Add(Entities.EntityExtensions.AsEntityMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { copy }));
            }

            _entityContextProvider.EntityContext.Commit();
            return Task.FromResult((object)output);
        }
    }
}