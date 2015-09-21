using System;
using System.Collections.Generic;
using System.IO;

namespace URSA.Web
{
    /// <summary>Represents a response.</summary>
    public interface IResponseInfo : IDisposable
    {
        /// <summary>Gets the corresponding request.</summary>
        IRequestInfo Request { get; }

        /// <summary>Gets the response headers.</summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>Gets the response body.</summary>
        Stream Body { get; }
    }
}