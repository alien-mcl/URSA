using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a basic support for HTTP OPTIONS requests.</summary>
    public class OptionsController : IController
    {
        private readonly string[] _allowed;

        internal OptionsController(params string[] allowed)
        {
            _allowed = allowed;
        }

        /// <inheritdoc />
        public IResponseInfo Response { get; set; }

        internal static OperationInfo<Verb> CreateOperationInfo(OperationInfo<Verb> sourceOperation)
        {
            return new OperationInfo<Verb>(
                typeof(OptionsController).GetMethod("Allow", BindingFlags.Instance | BindingFlags.NonPublic),
                sourceOperation.Uri,
                sourceOperation.UriTemplate,
                sourceOperation.TemplateRegex,
                Verb.OPTIONS,
                sourceOperation.Arguments.ToArray());
        }

        private void Allow()
        {
            ResponseInfo response = (ResponseInfo)Response;
            ((IDictionary<string, string>)response.Headers)["Allow"] = String.Join(", ", _allowed);
        }
    }
}