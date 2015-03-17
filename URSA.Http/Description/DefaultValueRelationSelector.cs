using System;
using System.Linq;
using System.Reflection;
using URSA.Web.Http;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    /// <summary></summary>
    public class DefaultValueRelationSelector : IDefaultValueRelationSelector
    {
        private static readonly string[] PopularIdentifierPropertyNames = new string[]
        {
            "Id",
            "Identifier",
            "Identity",
            "Key"
        };

        /// <inheritdoc />
        public ParameterSourceAttribute ProvideDefault(ParameterInfo parameter, Verb verb)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return GetDefaultParameterSource(parameter);
        }

        /// <inheritdoc />
        public ResultTargetAttribute ProvideDefault(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return GetDefaultResultTarget(parameter);
        }

        private static ParameterSourceAttribute GetDefaultParameterSource(ParameterInfo parameter)
        {
            if ((typeof(Guid) == parameter.ParameterType) || (typeof(DateTime) == parameter.ParameterType) ||
                ((IsIdentity(parameter.ParameterType)) &&
                (PopularIdentifierPropertyNames.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase))))
            {
                return FromUriAttribute.For(parameter);
            }
            
            if ((!parameter.ParameterType.IsValueType) && (typeof(string) != parameter.ParameterType) &&
                (!((parameter.ParameterType.IsEnumerable()) && (IsNumber(parameter.ParameterType.GetItemType())))))
            {
                return FromBodyAttribute.For(parameter);
            }
            
            return FromQueryStringAttribute.For(parameter);
        }

        private static ResultTargetAttribute GetDefaultResultTarget(ParameterInfo parameter)
        {
            if ((typeof(Guid) == parameter.ParameterType) || (typeof(DateTime) == parameter.ParameterType) ||
                ((IsIdentity(parameter.ParameterType)) &&
                (PopularIdentifierPropertyNames.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase))))
            {
                return new ToHeaderAttribute();
            }

            return new ToBodyAttribute();
        }

        private static bool IsIdentity(Type type)
        {
            return ((typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type));
        }

        private static bool IsNumber(Type type)
        {
            return (typeof(SByte) == type) || (typeof(Byte) == type) || (typeof(Int16) == type) || (typeof(UInt16) == type) ||
                (typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type) ||
                (typeof(Single) == type) || (typeof(Double) == type) || (typeof(Decimal) == type);
        }
    }
}