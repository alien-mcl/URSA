using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Description;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Tests.Testing
{
    /// <summary>Provides useful <see cref="MethodInfo" /> extensions.</summary>
    public static class MethodInfoExtensions
    {
        private static readonly string[] PopularIdentifierPropertyNames = { "Id", "Identifier", "Identity", "Key" };

        /// <summary>Converts a method into an operation descriptor..</summary>
        /// <typeparam name="T">Type of the protocol specific command.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="baseUri">The call URI.</param>
        /// <param name="verb">The verb.</param>
        /// <returns>Operation descriptor.</returns>
        public static OperationInfo<T> ToOperationInfo<T>(this MethodInfo method, string baseUri, T verb)
        {
            string callUri;
            return method.ToOperationInfo(baseUri, verb, out callUri);
        }

        /// <summary>Converts a method into an operation descriptor..</summary>
        /// <typeparam name="T">Type of the protocol specific command.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="baseUri">The call URI.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="callUri">Call URI.</param>
        /// <param name="values">Call values.</param>
        /// <returns>Operation descriptor.</returns>
        public static OperationInfo<T> ToOperationInfo<T>(this MethodInfo method, string baseUri, T verb, out string callUri, params object[] values)
        {
            var methodUri = method.GetCustomAttributes<RouteAttribute>().Select(attribute => attribute.Url.ToString()).FirstOrDefault() ?? method.Name.ToLower();
            if ((methodUri == "list") || (methodUri == "get") || (methodUri == "create") || (methodUri == "post") || 
                (methodUri == "update") || (methodUri == "put") || (methodUri == "delete"))
            {
                methodUri = String.Empty;
            }

            var actualCallUri = "/" + baseUri.Trim('/') + (methodUri.Length > 0 ? "/" + methodUri.TrimStart('/') : String.Empty);
            var targetUriTemplate = actualCallUri;
            var queryString = String.Empty;
            var arguments = method.GetParameters()
                .Where(parameter => !parameter.IsOut)
                .Select((parameter, index) =>
                {
                    string uriTemplate = null;
                    var target = parameter.GetParameterTarget();
                    if (target is FromUrlAttribute)
                    {
                        var temp = String.Format("/{{{0}}}", parameter.Name);
                        uriTemplate = (methodUri += temp);
                        targetUriTemplate += temp;
                        actualCallUri += temp;
                    }
                    else if (target is FromQueryStringAttribute)
                    {
                        queryString += (queryString.Length == 0 ? "?" : "&") + String.Format("{0}={{{0}}}", parameter.Name);
                        uriTemplate = methodUri + queryString;
                    }

                    return (ValueInfo)new ArgumentInfo(parameter, target, uriTemplate, (target is FromBodyAttribute ? null : parameter.Name));
                })
                .Concat(method.GetParameters()
                .Where(parameter => parameter.IsOut)
                .Select(parameter => (ValueInfo)new ResultInfo(parameter, parameter.GetResultTarget(), null, null)));
            if (method.ReturnParameter != null)
            {
                arguments = arguments.Concat(new[] { new ResultInfo(method.ReturnParameter, method.ReturnParameter.GetResultTarget(), null, null) });
            }

            arguments = arguments.ToArray();
            callUri = actualCallUri + queryString;
            targetUriTemplate += queryString;
            var queryStringParameters = Regex.Matches(callUri, "[?&]([^=]+)=[^&]+").Cast<Match>();
            var queryStringRegex = (queryStringParameters.Any() ? "[?&](" + String.Join("|", queryStringParameters.Select(item => item.Groups[1].Value)) + ")=[^&]+" : String.Empty);
            var methodRegex = new Regex("^" + Regex.Replace(actualCallUri, "/{[^}]+}", "/[^\\/]+") + queryStringRegex + "$");
            return new OperationInfo<T>(method, (HttpUrl)UrlParser.Parse("/" + methodUri), targetUriTemplate, methodRegex, verb, (ValueInfo[])arguments);
        }

        private static ParameterSourceAttribute GetParameterTarget(this ParameterInfo parameter)
        {
            var explicitSetting = parameter.GetCustomAttribute<ParameterSourceAttribute>(true);
            if (explicitSetting != null)
            {
                return explicitSetting;
            }

            if ((typeof(Guid) == parameter.ParameterType) || (typeof(DateTime) == parameter.ParameterType) ||
                ((IsIdentity(parameter.ParameterType)) &&
                (PopularIdentifierPropertyNames.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase))))
            {
                return FromUrlAttribute.For(parameter);
            }

            var parameterTypeInfo = parameter.ParameterType.GetTypeInfo();
            if ((parameterTypeInfo.IsValueType) && (typeof(string) != parameter.ParameterType) &&
                (!((parameterTypeInfo.IsEnumerable()) && (IsNumber(parameterTypeInfo.GetItemType())))))
            {
                return FromBodyAttribute.For(parameter);
            }

            return FromQueryStringAttribute.For(parameter);
        }

        private static ResultTargetAttribute GetResultTarget(this ParameterInfo parameter)
        {
            var explicitSetting = parameter.GetCustomAttribute<ResultTargetAttribute>(true);
            if (explicitSetting != null)
            {
                return explicitSetting;
            }

            if ((typeof(Guid).MakeByRefType() == parameter.ParameterType) || (typeof(DateTime).MakeByRefType() == parameter.ParameterType) ||
                ((IsIdentity(parameter.ParameterType)) &&
                (PopularIdentifierPropertyNames.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase))))
            {
                return new ToHeaderAttribute(Header.Location);
            }

            return new ToBodyAttribute();
        }

        private static bool IsIdentity(Type type)
        {
            return ((typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type) ||
                (typeof(Int32).MakeByRefType() == type) || (typeof(UInt32).MakeByRefType() == type) || (typeof(Int64).MakeByRefType() == type) || (typeof(UInt64).MakeByRefType() == type));
        }

        private static bool IsNumber(Type type)
        {
            return (typeof(SByte) == type) || (typeof(Byte) == type) || (typeof(Int16) == type) || (typeof(UInt16) == type) ||
                (typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type) ||
                (typeof(Single) == type) || (typeof(Double) == type) || (typeof(Decimal) == type);
        }
    }
}