using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using URSA.Web.Http.Description;
using URSA.Web.Http.Description.Entities;

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

        /// <inheritdoc />
        protected override string GetComponentName(MethodInfo method, object[] arguments)
        {
            if ((MethodMatchesCriteria<IEntityContextProvider>(method, "get_MetaGraph")) ||
                (MethodMatchesCriteria<IEntityContextProvider>(method, "get_EntityContext")) ||
                (MethodMatchesCriteria<IEntityContextProvider>(method, "get_TripleStore")))
            {
                return "InMemory" + method.Name.Substring(3).TrimStart('_');
            }

            return base.GetComponentName(method, arguments);
        }

        private bool MethodMatchesCriteria<T>(MethodInfo method, string name, int numberOfParameters = 0)
        {
            return (method.Name == name) && (method.DeclaringType == typeof(T)) && (method.GetParameters().Length == numberOfParameters);
        }
    }
}