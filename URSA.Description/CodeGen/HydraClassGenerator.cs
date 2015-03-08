using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RomanticWeb;
using RomanticWeb.Entities;
using URSA.Web;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Model;

namespace URSA.CodeGen
{
    /// <summary>Provides a basic implementation of the <see cref="IClassGenerator" />.</summary>
    public class HydraClassGenerator : IClassGenerator
    {
        private const string ClassTemplate =
            @"using System;
using URSA.Web.Http;

namespace {0}
{{
    public class {1} : Client
    {{
        public {1}(Uri baseUri) : base(baseUri)
        {{
        }}

{2}
    }}
}}";

        private const string OperationTemplate =
            @"        public void {0}({1})
        {{
            Call(Verb.{2}, new Uri(""{3}""){4});
        }}
";

        private static readonly IDictionary<IResource, string> Namespaces = new ConcurrentDictionary<IResource, string>();
        private static readonly IDictionary<IResource, string> Names = new ConcurrentDictionary<IResource, string>();

        private readonly IEnumerable<IUriParser> _uriParsers;

        /// <summary>Initializes a new instance of the <see cref="HydraClassGenerator"/> class.</summary>
        /// <param name="uriParsers">The URI parsers.</param>
        public HydraClassGenerator(IEnumerable<IUriParser> uriParsers)
        {
            if (uriParsers == null)
            {
                throw new ArgumentNullException("uriParsers");
            }

            _uriParsers = uriParsers;
        }

        /// <inheritdoc />
        public string CreateCode(IClass supportedClass)
        {
            var operations = new StringBuilder(1024);
            var supportedOperations = from quad in supportedClass.Context.Store.Quads
                                      where (quad.Subject.IsUri) && (AbsoluteUriComparer.Default.Equals(quad.Subject.Uri, supportedClass.Id.Uri)) &&
                                            (quad.Object.IsUri) && (quad.Predicate.IsUri)
                                      let isTemplate = quad.PredicateIs(supportedClass.Context.Store, new Uri(HydraUriParser.HyDrA + "IriTemplate"))
                                      where ((quad.Predicate.Uri.ToString() == HydraUriParser.HyDrA + "supportedOperation") || (isTemplate))
                                      select new
                                      {
                                          Id = new EntityId(quad.Object.Uri),
                                          Template = (isTemplate ? supportedClass.Context.Load<IIriTemplate>(new EntityId(quad.Predicate.Uri)) : null)
                                      };
            foreach (var operationDescriptor in supportedOperations)
            {
                var operation = supportedClass.Context.Load<IOperation>(operationDescriptor.Id);
                foreach (var method in operation.Method)
                {
                    var arguments = new StringBuilder(256);
                    var parameters = new StringBuilder(256);
                    foreach (var expected in operation.Expects)
                    {
                        string variableName = expected.Label.ToLowerCamelCase();
                        parameters.AppendFormat("{0}.{1} {2}", CreateNamespace(expected), CreateName(expected), variableName);
                        arguments.Append(variableName);
                    }

                    if (arguments.Length > 0)
                    {
                        arguments.Insert(0, ", ");
                    }

                    operations.AppendFormat(OperationTemplate, CreateName(operation), parameters, method, operation.Id, arguments);
                }
            }

            return String.Format(ClassTemplate, CreateNamespace(supportedClass), CreateName(supportedClass), operations);
        }

        /// <inheritdoc />
        public string CreateNamespace(IResource resource)
        {
            string result;
            if (Namespaces.TryGetValue(resource, out result))
            {
                return result;
            }

            ParseUri(resource);
            return Namespaces[resource];
        }

        /// <inheritdoc />
        public string CreateName(IResource resource)
        {
            string result;
            if (Names.TryGetValue(resource, out result))
            {
                return result;
            }

            ParseUri(resource);
            return Names[resource];
        }

        /// <inheritdoc />
        private string CreateName(IClass @class)
        {
            return (!String.IsNullOrEmpty(@class.Label) ? @class.Label : CreateName((IResource)@class));
        }

        /// <inheritdoc />
        private string CreateName(IOperation operation)
        {
            return (!String.IsNullOrEmpty(operation.Label) ? operation.Label : CreateName((IResource)operation));
        }

        private void ParseUri(IResource resource)
        {
            var uriParser = (from parser in _uriParsers
                             orderby parser.IsApplicable(resource.Id.Uri) descending 
                             select parser).FirstOrDefault();

            if (uriParser == null)
            {
                throw new InvalidOperationException(String.Format("Cannot find a suitable parser for resource uri '{0}'.", resource.Id.Uri));
            }

            string @namespace;
            Names[resource] = uriParser.Parse(resource.Id.Uri, out @namespace);
            Namespaces[resource] = @namespace;
        }
    }
}