using System;
using System.Linq;
using RomanticWeb.Entities;
using URSA.Web.Description;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    internal static class OperationExtensions
    {
        internal static bool IsWriteControllerOperation<T>(this OperationInfo<T> operation)
        {
            Type type;
            return (((type = operation.UnderlyingMethod.DeclaringType.GetInterfaces().FirstOrDefault(IsWriteControllerOperation)) != null) &&
                (operation.UnderlyingMethod.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(operation.UnderlyingMethod)));
        }

        internal static EntityId CreateId<T>(this OperationInfo<T> operation, Uri baseUri)
        {
            HttpUrl url = (HttpUrl)baseUri + (HttpUrl)operation.Url;
            if (!operation.Arguments.Any())
            {
                return new EntityId((Uri)url);
            }

            var fragment = String.Join("And", operation.Arguments.Select(argument => (argument.VariableName ?? argument.Parameter.Name).ToUpperCamelCase()));
            url = url.WithFragment(String.Format("{0}{1}", operation.ProtocolSpecificCommand, fragment));
            return new EntityId((Uri)url);
        }

        internal static IOperation AsOperation<T>(this OperationInfo<T> operation, IApiDocumentation apiDocumentation, EntityId id = null)
        {
            var methodId = id ?? operation.CreateId(apiDocumentation.Context.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative))));
            if (operation.IsWriteControllerOperation())
            {
                switch (operation.UnderlyingMethod.Name)
                {
                    case "Delete":
                        return apiDocumentation.Context.Create<IDeleteResourceOperation>(methodId);
                    case "Update":
                        return apiDocumentation.Context.Create<IReplaceResourceOperation>(methodId);
                    case "Create":
                        return apiDocumentation.Context.Create<ICreateResourceOperation>(methodId);
                }
            }

            return apiDocumentation.Context.Create<IOperation>(methodId);
        }

        private static bool IsWriteControllerOperation(Type @interface)
        {
            return (@interface.IsGenericType) && (typeof(IWriteController<,>).IsAssignableFrom(@interface.GetGenericTypeDefinition()));
        }
    }
}