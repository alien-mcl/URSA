using System;
using System.Collections.Generic;

namespace URSA.Web.Converters
{
    /// <summary>Provides a basic interface for <see cref="IConverter" /> providers.</summary>
    public interface IConverterProvider
    {
        /// <summary>Initializes provider with converters.</summary>
        /// <param name="converters">Converters to acknowledge.</param>
        void Initialize(IEnumerable<IConverter> converters);

        /// <summary>Searches for the best matching input <see cref="IConverter" />.</summary>
        /// <typeparam name="T">Type of the expected type.</typeparam>
        /// <param name="request">Request details.</param>
        /// <param name="ignoreProtocol">Instructs to ignore protocol matching.</param>
        /// <returns>Instance of the <see cref="IConverter" /> if possible; otherwise <b>null</b>.</returns>
        IConverter FindBestInputConverter<T>(IRequestInfo request, bool ignoreProtocol = false);

        /// <summary>Searches for the best matching input <see cref="IConverter" />.</summary>
        /// <param name="expectedType">Type of the expected type.</param>
        /// <param name="request">Request details.</param>
        /// <param name="ignoreProtocol">Instructs to ignore protocol matching.</param>
        /// <returns>Instance of the <see cref="IConverter" /> if possible; otherwise <b>null</b>.</returns>
        IConverter FindBestInputConverter(Type expectedType, IRequestInfo request, bool ignoreProtocol = false);

        /// <summary>Searches for the best matching output <see cref="IConverter" />.</summary>
        /// <typeparam name="T">Type of the expected type.</typeparam>
        /// <param name="response">Response details.</param>
        /// <returns>Instance of the <see cref="IConverter" /> if possible; otherwise <b>null</b>.</returns>
        IConverter FindBestOutputConverter<T>(IResponseInfo response);

        /// <summary>Searches for the best matching output <see cref="IConverter" />.</summary>
        /// <param name="expectedType">Type of the expected type.</param>
        /// <param name="response">Response details.</param>
        /// <returns>Instance of the <see cref="IConverter" /> if possible; otherwise <b>null</b>.</returns>
        IConverter FindBestOutputConverter(Type expectedType, IResponseInfo response);
    }
}