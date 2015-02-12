﻿using RomanticWeb;
using RomanticWeb.Entities;
using System;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a ReST description facility.</summary>
    /// <typeparam name="T">Type of the controller to generate documentation for.</typeparam>
    [DependentRoute]
    public class DescriptionController<T> : IController where T : IController
    {
        private static ApiDescriptionBuilder<T> apiDescriptionBuilder;

        private IEntityContext _entityContext;

        /// <summary>Initializes a new instance of the <see cref="DescriptionController{T}" /> class.</summary>
        /// <param name="entityContext">Entity context.</param>
        /// <param name="handlerMapper">Handler mapper instance.</param>
        public DescriptionController(IEntityContext entityContext, IDelegateMapper<RequestInfo> handlerMapper)
        {
            if (entityContext == null)
            {
                throw new ArgumentNullException("entityContext");
            }

            if ((apiDescriptionBuilder == null) && (handlerMapper == null))
            {
                throw new ArgumentNullException("handlerMapper");
            }

            if ((apiDescriptionBuilder == null) && (!(handlerMapper is DelegateMapper)))
            {
                throw new ArgumentOutOfRangeException("handlerMapper");
            }

            _entityContext = entityContext;
            if (apiDescriptionBuilder == null)
            {
                apiDescriptionBuilder = new ApiDescriptionBuilder<T>((DelegateMapper)handlerMapper);
            }
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        /// <summary>Gets the API documentation.</summary>
        /// <returns><see cref="IApiDocumentation" /> instance.</returns>
        [Route("/")]
        [OnOptions]
        public IApiDocumentation GetApiEntryPointDescription()
        {
            IApiDocumentation result = _entityContext.Create<IApiDocumentation>(new EntityId(Response.Request.Uri));
            apiDescriptionBuilder.BuildDescription(result);
            return result;
        }
    }
}