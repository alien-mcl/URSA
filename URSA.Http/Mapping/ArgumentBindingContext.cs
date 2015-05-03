using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Describes an argument binding context.</summary>
    [ExcludeFromCodeCoverage]
    public abstract class ArgumentBindingContext
    {
        internal ArgumentBindingContext(RequestInfo request, RequestMapping requestMapping, ParameterInfo parameter, int index, IDictionary<RequestInfo, RequestInfo[]> multipartBodies)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (requestMapping == null)
            {
                throw new ArgumentNullException("requestMapping");
            }

            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (multipartBodies == null)
            {
                throw new ArgumentNullException("multipartBodies");
            }

            Request = request;
            RequestMapping = requestMapping;
            Parameter = parameter;
            Index = index;
            MultipartBodies = multipartBodies;
        }

        internal RequestInfo Request { get; private set; }
            
        internal RequestMapping RequestMapping { get; private set; }
        
        internal ParameterInfo Parameter { get; private set; }
        
        internal int Index { get; private set; }
        
        internal IDictionary<RequestInfo, RequestInfo[]> MultipartBodies { get; private set; }

        internal bool Success { get; set; }
    }

    /// <summary>Describes an argument binding in specific parameter source context.</summary>
    [ExcludeFromCodeCoverage]
    public class ArgumentBindingContext<T> : ArgumentBindingContext where T : ParameterSourceAttribute
    {
        internal ArgumentBindingContext(RequestInfo request, RequestMapping requestMapping, ParameterInfo parameter, int index, T parameterSource, IDictionary<RequestInfo, RequestInfo[]> multipartBodies) :
            base(request, requestMapping, parameter, index, multipartBodies)
        {
            if (parameterSource == null)
            {
                throw new ArgumentNullException("parameterSource");
            }

            ParameterSource = parameterSource;
        }

        internal T ParameterSource { get; private set; }
    }
}