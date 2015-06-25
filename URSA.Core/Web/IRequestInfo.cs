using System;
using System.Collections.Generic;
using System.IO;

namespace URSA.Web
{
    /// <summary>Describes a request.</summary>
    public interface IRequestInfo
    {
        /// <summary>Gets a value indicating whether the output can be provided by any of the converters available.</summary>
        bool OutputNeutral { get; }

        /// <summary>Gets the request headers.</summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>Gets the body of the request.</summary>
        Stream Body { get; }

        /// <summary>Gets the uri of the request.</summary>
        Uri Uri { get; }
    }
}