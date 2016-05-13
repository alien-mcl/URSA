using System;
using System.Collections.Generic;
using System.IO;
using URSA.Security;
using URSA.Web.Http;

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

        /// <summary>Gets the URL of the request.</summary>
        Url Url { get; }

        /// <summary>Gets or sets the identity of this request.</summary>
        /// <exception cref="ArgumentNullException">Thrown when setting a <b>null</b> value.</exception>
        IClaimBasedIdentity Identity { get; set; }
    }
}