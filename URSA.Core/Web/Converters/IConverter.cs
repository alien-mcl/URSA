using System;
using System.Collections.Generic;

namespace URSA.Web.Converters
{
    /// <summary>Defines body-converter compatibility levels.</summary>
    [Flags]
    public enum CompatibilityLevel : int
    {
        /// <summary>Defines a both type and protocol exact match of the converter against given body.</summary>
        ExactMatch = 0x00000033,

        /// <summary>Defines an exact type match of the converter against given body.</summary>
        ExactTypeMatch = 0x00000030,

        /// <summary>Defines an exact protocol match of the converter against given body.</summary>
        ExactProtocolMatch = 0x00000003,

        /// <summary>Defines a type match of the converter against given body.</summary>
        TypeMatch = 0x00000010,

        /// <summary>Defines a media type match of the converter against given body.</summary>
        ProtocolMatch = 0x00000001,

        /// <summary>Defines a lack of compatibility of the converter against given body.</summary>
        None = 0x00000000
    }

    /// <summary>Provides a basic value conversion facility.</summary>
    public interface IConverter
    {
        /// <summary>Gets the supported media types.</summary>
        IEnumerable<string> SupportedMediaTypes { get; }

        /// <summary>Checks if the converter can convert body of the request to the expected type.</summary>
        /// <typeparam name="T">Type of object expected.</typeparam>
        /// <param name="request">Request details.</param>
        /// <returns><see cref="CompatibilityLevel" /> between given request body and converter.</returns>
        CompatibilityLevel CanConvertTo<T>(IRequestInfo request);

        /// <summary>Checks if the converter can convert body of the request to the expected type.</summary>
        /// <param name="expectedType">Type of object expected.</param>
        /// <param name="request">Request details.</param>
        /// <returns><see cref="CompatibilityLevel" /> between given request body and converter.</returns>
        CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request);

        /// <summary>Converts the request body into an object of the expected type.</summary>
        /// <typeparam name="T">Type of object expected.</typeparam>
        /// <param name="request">Request details.</param>
        /// <returns>Object being an programmatic representation of the request body.</returns>
        T ConvertTo<T>(IRequestInfo request);

        /// <summary>Converts the request body into an object of the expected type.</summary>
        /// <param name="expectedType">Type of object expected.</param>
        /// <param name="request">Request details.</param>
        /// <returns>Object being an programmatic representation of the request body.</returns>
        object ConvertTo(Type expectedType, IRequestInfo request);

        /// <summary>Converts the request body into an object of the expected type.</summary>
        /// <typeparam name="T">Type of object expected.</typeparam>
        /// <param name="body">Message body string.</param>
        /// <returns>Object being an programmatic representation of the request body.</returns>
        T ConvertTo<T>(string body);

        /// <summary>Converts the request body into an object of the expected type.</summary>
        /// <param name="expectedType">Type of object expected.</param>
        /// <param name="body">Message body string.</param>
        /// <returns>Object being an programmatic representation of the request body.</returns>
        object ConvertTo(Type expectedType, string body);

        /// <summary>Checks if the converter can convert object to response message body.</summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="response">Response details.</param>
        /// <returns><see cref="CompatibilityLevel" /> between desired type of object passed to the converter and the response.</returns>
        CompatibilityLevel CanConvertFrom<T>(IResponseInfo response);

        /// <summary>Checks if the converter can convert object to response message body.</summary>
        /// <param name="givenType">Type of object.</param>
        /// <param name="response">Response details.</param>
        /// <returns><see cref="CompatibilityLevel" /> between desired type of object passed to the converter and the response.</returns>
        CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response);

        /// <summary>Converts an instance of the object into a response message body.</summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="instance">Instance to be converted.</param>
        /// <param name="response">Response descriptor.</param>
        void ConvertFrom<T>(T instance, IResponseInfo response);

        /// <summary>Converts an instance of the object into a response message body.</summary>
        /// <param name="givenType">Type of object.</param>
        /// <param name="instance">Instance to be converted.</param>
        /// <param name="response">Response descriptor.</param>
        void ConvertFrom(Type givenType, object instance, IResponseInfo response);
    }
}