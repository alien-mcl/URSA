﻿using System;
using System.Diagnostics;
using System.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description
{
    /// <summary>Describes a parameter.</summary>
    [DebuggerDisplay("{Parameter}", Name = "{Parameter}")]
    public class ArgumentInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ArgumentInfo" /> class.</summary>
        /// <param name="parameter">Actual underlying parameter.</param>
        /// <param name="source">Parameter source.</param>
        /// <param name="uriTemplate">Relative uri template of this parameter.</param>
        /// <param name="variableName">Variable name in the template for given argument</param>
        public ArgumentInfo(ParameterInfo parameter, ParameterSourceAttribute source, string uriTemplate, string variableName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if ((variableName != null) && (variableName.Length > 0))
            {
                if (uriTemplate == null)
                {
                    throw new ArgumentNullException("uriTemplate");
                }

                if (uriTemplate.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("uriTemplate");
                }
            }

            if ((uriTemplate != null) && (uriTemplate.Length > 0))
            {
                if (variableName == null)
                {
                    throw new ArgumentNullException("variableName");
                }

                if (variableName.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("variableName");
                }
            }

            Parameter = parameter;
            Source = source;
            UriTemplate = uriTemplate;
            VariableName = variableName;
        }

        /// <summary>Gets the underlying parameter.</summary>
        public ParameterInfo Parameter { get; private set; }

        /// <summary>Gets the parameter source.</summary>
        public ParameterSourceAttribute Source { get; private set; }

        /// <summary>Gets the relative uri template of this argument.</summary>
        public string UriTemplate { get; private set; }

        /// <summary>Gets the variable name in the template for given argument.</summary>
        public string VariableName { get; private set; }

        /// <summary>Gets or sets the owning method.</summary>
        internal MethodInfo Method { get; set; }
    }
}