using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RDeF.Entities;
using RollerCaster;
using URSA.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.Hydra;
using ICollection = URSA.Web.Http.Description.Hydra.ICollection;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a <![CDATA[hydra:Collection]]> injection mechanism.</summary>
    public class CollectionResponseModelTransformer : IResponseModelTransformer<RdfPayloadModelTransformer>
    {
        private readonly IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="CollectionResponseModelTransformer"/> class.</summary>
        /// <param name="entityContext">The entity context.</param>
        [ExcludeFromCodeCoverage]
        public CollectionResponseModelTransformer(IEntityContext entityContext)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            _entityContext = entityContext;
        }

        /// <inheritdoc />
        public Task<object> Transform(IRequestMapping requestMapping, IRequestInfo request, object result, object[] arguments)
        {
            RequestInfo requestInfo;
            MethodInfo underlyingMethod;
            if (!CanOutputHypermedia((underlyingMethod = requestMapping.Operation.UnderlyingMethod).ReturnType, requestInfo = request as RequestInfo))
            {
                return Task.FromResult(result);
            }

            int totalItems = ((IEnumerable)result).Cast<object>().Count();
            int skip = 0;
            int take = 0;
            bool canOutputHypermedia = false;
            KeyValuePair<Verb, MethodInfo> method;
            if ((requestMapping.Target.GetType().GetTypeInfo().GetImplementationOfAny(typeof(IController<>), typeof(IAsyncController<>)) != null) &&
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
                            canOutputHypermedia = (requestMapping.ArgumentSources[1] == ArgumentValueSources.Bound) && (requestMapping.ArgumentSources[2] == ArgumentValueSources.Bound);
                        }

                        break;
                }
            }

            if (!canOutputHypermedia)
            {
                return Task.FromResult(result);
            }

            result = TransformCollection(result, requestInfo.Url, totalItems, skip, take);
            return Task.FromResult(result);
        }

        private object TransformCollection(object result, HttpUrl requestUri, int totalItems, int skip, int take)
        {
            var entityContext = _entityContext;
            var collection = entityContext.Load<ICollection>((Uri)requestUri);
            collection.TotalItems = totalItems;
            collection.Members.Clear();
            foreach (IEntity entity in (IEnumerable)result)
            {
                collection.Members.Add(entity.ActLike<IResource>());
            }

            if ((skip <= 0) && (take <= 0))
            {
                entityContext.Commit();
                return result;
            }

            var viewId = ((Uri)collection.Iri).AddFragment("view");
            var view = entityContext.Load<IPartialCollectionView>(viewId);
            collection.View = view;
            view.ItemsPerPage = (take > 0 ? take : totalItems);
            entityContext.Commit();
            return result;
        }

        private bool CanOutputHypermedia(Type returnType, RequestInfo requestInfo)
        {
            var matchingMediaTypes = from accepted in requestInfo.Headers[Header.Accept].Values
                                     join supportedMediaType in EntityConverter.MediaTypes.Concat(new[] { "*/*" }) on accepted.Value equals supportedMediaType
                                     select supportedMediaType;
            return
                ((DescriptionConfigurationSection.Default.HypermediaMode == HypermediaModes.SameGraph) &&
                (returnType.GetTypeInfo().IsEnumerable()) &&
                (requestInfo != null) && ((requestInfo.Headers[Header.Accept] != null) && (matchingMediaTypes.Any())));
        }
    }
}