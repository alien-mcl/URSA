using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using URSA.Web.Http.Description;

namespace URSA.CastleWindsor.ComponentModel
{
    internal class UrsaCustomTypedFactory : DefaultTypedFactoryComponentSelector
    {
        /// <inheritdoc />
        protected override Type GetComponentType(MethodInfo method, object[] arguments)
        {
            if ((method.Name == "Create") && (method.DeclaringType == typeof(IApiDescriptionBuilderFactory)) && (method.GetParameters().Length == 1))
            {
                return typeof(IApiDescriptionBuilder<>).MakeGenericType((Type)arguments[0]);
            }

            return base.GetComponentType(method, arguments);
        }
    }
}