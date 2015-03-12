using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Http;

namespace URSA.Web.Description.Http
{
    /// <summary>Describes an HTTP controller operation.</summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Verb} {TemplateRegex}", Name = "{TemplateRegex}")]
    public class OperationInfo : URSA.Web.Description.OperationInfo
    {
        /// <summary>Initializes a new instance of the <see cref="OperationInfo" /> class.</summary>
        /// <param name="underlyingMethod">Actual underlying method.</param>
        /// <param name="verb">An HTTP verb for given operation</param>
        /// <param name="uri">Base relative uri of the method without arguments.</param>
        /// <param name="templateRegex">Uri template regular expression.</param>
        /// <param name="uriTemplate">Relative uri template with all arguments included.</param>
        /// <param name="arguments">Argument descriptions.</param>
        public OperationInfo(MethodInfo underlyingMethod, Verb verb, Uri uri, Regex templateRegex, string uriTemplate, params ArgumentInfo[] arguments) : base(underlyingMethod, uri, uriTemplate, arguments)
        {
            if (verb == null)
            {
                throw new ArgumentNullException("verb");
            }

            Verb = verb;
            TemplateRegex = templateRegex;
        }

        /// <summary>Gets the HTTP verb for given operation.</summary>
        public Verb Verb { get; private set; }

        /// <summary>Gets the uri template regular expression,</summary>
        internal Regex TemplateRegex { get; private set; }
    }
}