using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Instructs the pipeline to output the result as the given media type.</summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class AsMediaTypeAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="AsMediaTypeAttribute" /> class.</summary>
        /// <param name="mediaType">Target media type.</param>
        /// <param name="parameters">Optional parameters of the media type.</param>
        public AsMediaTypeAttribute(string mediaType, params string[] parameters)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (mediaType.Length == 0)
            {
                throw new ArgumentOutOfRangeException("mediaType");
            }

            MediaType = mediaType;
            var headerParameters = new HeaderParameterCollection();
            parameters.ForEach(parameter => headerParameters.Add(HeaderParameter.Parse(parameter)));
            Parameters = headerParameters;
        }

        /// <summary>Gets the target media type.</summary>
        public string MediaType { get; private set; }

        /// <summary>Gets the parameters of the media type.</summary>
        public IEnumerable<HeaderParameter> Parameters { get; private set; }
    }
}