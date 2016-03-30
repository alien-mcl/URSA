using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RomanticWeb.Entities;
using URSA.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using ICollection = URSA.Web.Http.Description.Hydra.ICollection;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a <![CDATA[hydra:Collection]]> injection mechanism.</summary>
    public class CollectionModelTransformer : IModelTransformer
    {
        private readonly IEntityContextProvider _entityContextProvider;

        /// <summary>Initializes a new instance of the <see cref="CollectionModelTransformer"/> class.</summary>
        /// <param name="entityContextProvider">The entity context provider.</param>
        [ExcludeFromCodeCoverage]
        public CollectionModelTransformer(IEntityContextProvider entityContextProvider)
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
            RequestInfo requestInfo;
            MethodInfo underlyingMethod;
            if (!CanOutputHypermedia((underlyingMethod = requestMapping.Operation.UnderlyingMethod).ReturnType, requestInfo = request as RequestInfo))
            {
                return result;
            }

            int totalItems = ((IEnumerable)result).Cast<object>().Count();
            int skip = 0;
            int take = 0;
            KeyValuePair<Verb, MethodInfo> method;
            if ((requestMapping.Target.GetType().GetImplementationOfAny(typeof(IController<>), typeof(IAsyncController<>)) != null) &&
                (!Equals(method = requestMapping.Target.GetType().DiscoverCrudMethods().FirstOrDefault(entry => entry.Value == underlyingMethod), default(KeyValuePair<Verb, MethodInfo>))))
            {
                switch (method.Key.ToString())
                {
                    case "":
                        var parameters = underlyingMethod.GetParameters();
                        var resultingValues = arguments.Where((item, index) => parameters[index].IsOut).ToList();
                        totalItems = (resultingValues.Count > 0 ? (int)resultingValues[0] : -1);
                        if ((arguments[1] != null) && (arguments[2] != null))
                        {
                            skip = (int)arguments[1];
                            take = ((take = (int)arguments[2]) == 0 ? 0 : Math.Min(take, totalItems));
                        }

                        break;
                }
            }

            return await TransformCollection(result, requestInfo.Uri, totalItems, skip, take);
        }

        private async Task<object> TransformCollection(object result, Uri requestUri, int totalItems, int skip, int take)
        {
            var collection = _entityContextProvider.EntityContext.Load<ICollection>(requestUri);
            collection.TotalItems = totalItems;
            foreach (IEntity entity in (IEnumerable)result)
            {
                collection.Members.Add(entity.AsEntity<IResource>());
            }

            if ((skip <= 0) && (take <= 0))
            {
                return await Task.FromResult(result);
            }

            var view = _entityContextProvider.EntityContext.Load<IPartialCollectionView>(collection.CreateBlankId());
            collection.View = view;
            view.TotalItems = (take > 0 ? take : totalItems);
            return await Task.FromResult(result);
        }

        private bool CanOutputHypermedia(Type returnType, RequestInfo requestInfo)
        {
            var matchingMediaTypes = from accepted in requestInfo.Headers[Header.Accept].Values
                                     join supportedMediaType in EntityConverter.MediaTypes.Concat(new[] { "*/*" }) on accepted.Value equals supportedMediaType
                                     select supportedMediaType;
            return 
                ((DescriptionConfigurationSection.Default.HypermediaMode != HypermediaModes.SameGraph) || 
                (!System.Reflection.TypeExtensions.IsEnumerable(returnType)) ||
                (requestInfo == null) || 
                ((requestInfo.Headers[Header.Accept] != null) && (!matchingMediaTypes.Any())));
        }
    }
}