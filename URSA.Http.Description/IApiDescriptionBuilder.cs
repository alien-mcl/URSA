using System;
using System.Collections.Generic;
using RomanticWeb.Entities;
using URSA.Web.Description;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract of the API description building facility.</summary>
    public interface IApiDescriptionBuilder
    {
        /// <summary>Gets the type a given controller specializes in.</summary>
        Type SpecializationType { get; }

        /// <summary>Builds an API description.</summary>
        /// <param name="apiDocumentation">API documentation.</param>
        /// <param name="profiles">Requested media type profiles if any.</param>
        void BuildDescription(IApiDocumentation apiDocumentation, IEnumerable<Uri> profiles);

        /// <summary>Builds the operation description.</summary>
        /// <param name="context">The context.</param>
        /// <param name="operationInfo">Operation to be described.</param>
        /// <param name="profiles">Requested media type profiles if any.</param>
        void BuildOperationDescription(IEntity context, OperationInfo<Verb> operationInfo, IEnumerable<Uri> profiles);
    }

    /// <summary>Provides a contract of the API description building facility.</summary>
    /// <typeparam name="T">Type of the controller being described.</typeparam>
    public interface IApiDescriptionBuilder<T> : IApiDescriptionBuilder where T : IController
    {
    }
}