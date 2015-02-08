using System;
using System.Linq;
using System.Reflection;
using URSA.Web.Http;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    /// <summary></summary>
    public class DefaultParameterSourceSelector : IDefaultParameterSourceSelector
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

        private ParameterSourceAttribute GetDefaultParameterSource(ParameterInfo parameter)
        {
            Type itemType = parameter.ParameterType.GetItemType();
            if ((typeof(Guid) == parameter.ParameterType) || (typeof(DateTime) == parameter.ParameterType) ||
                ((IsIdentity(parameter.ParameterType)) &&
                (PopularIdentifierPropertyNames.Contains(parameter.Name, StringComparer.OrdinalIgnoreCase))))
            {
                return FromUriAttribute.For(parameter);
            }
            else if ((!parameter.ParameterType.IsValueType) &&
                (typeof(string) != parameter.ParameterType) &&
                (!((parameter.ParameterType.IsEnumerable()) && (IsNumber(itemType)))))
            {
                return FromBodyAttribute.For(parameter);
            }
            else
            {
                return FromQueryStringAttribute.For(parameter);
            }
        }

        private bool IsIdentity(Type type)
        {
            return ((typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type));
        }

        private bool IsNumber(Type type)
        {
            return (typeof(SByte) == type) || (typeof(Byte) == type) || (typeof(Int16) == type) || (typeof(UInt16) == type) ||
                (typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type) ||
                (typeof(Single) == type) || (typeof(Double) == type) || (typeof(Decimal) == type);
        }
    }
}