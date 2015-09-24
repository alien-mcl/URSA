using System;
using System.Linq;
using RomanticWeb.Entities;
using URSA.Web.Description;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    internal static class OperationExtensions
    {
        internal static bool IsDeleteOperation<T>(this OperationInfo<T> operation)
        {
            return (operation.IsWriteControllerOperation()) && (operation.UnderlyingMethod.Name == "Delete");
        }

        internal static bool IsUpdateOperation<T>(this OperationInfo<T> operation)
        {
            return (operation.IsWriteControllerOperation()) && (operation.UnderlyingMethod.Name == "Update");
        }

        internal static bool IsCreateOperation<T>(this OperationInfo<T> operation)
        {
            return (operation.IsWriteControllerOperation()) && (operation.UnderlyingMethod.Name == "Create");
        }

        internal static bool IsWriteControllerOperation<T>(this OperationInfo<T> operation)
        {
            Type type;
            return (((type = operation.UnderlyingMethod.DeclaringType.GetInterfaces().FirstOrDefault(IsWriteControllerOperation)) != null) &&
                (operation.UnderlyingMethod.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(operation.UnderlyingMethod)));
        }

        internal static EntityId CreateId<T>(this OperationInfo<T> operation, Uri baseUri)
        {
            Uri uri = operation.Uri.Combine(baseUri);
            if (!operation.Arguments.Any())
            {
                return new EntityId(uri);
            }

            var fragment = String.Join("And", operation.Arguments.Select(argument => (argument.VariableName ?? argument.Parameter.Name).ToUpperCamelCase()));
            uri = uri.AddFragment(String.Format("{0}{1}", operation.ProtocolSpecificCommand, fragment));
            return new EntityId(uri);
        }

        internal static IOperation AsOperation<T>(this OperationInfo<T> operation, IApiDocumentation apiDocumentation)
        {
            var methodId = operation.CreateId(apiDocumentation.Context.BaseUriSelector.SelectBaseUri(new EntityId(new Uri("/", UriKind.Relative))));
            return 
                (operation.IsCreateOperation() ? apiDocumentation.Context.Create<ICreateResourceOperation>(methodId) :
                (operation.IsDeleteOperation() ? apiDocumentation.Context.Create<IDeleteResourceOperation>(methodId) :
                (operation.IsUpdateOperation() ? apiDocumentation.Context.Create<IReplaceResourceOperation>(methodId) :
                 apiDocumentation.Context.Create<IOperation>(methodId))));
        }

        private static bool IsWriteControllerOperation(Type @interface)
        {
            return (@interface.IsGenericType) && (typeof(IWriteController<,>).IsAssignableFrom(@interface.GetGenericTypeDefinition()));
        }
    }
}