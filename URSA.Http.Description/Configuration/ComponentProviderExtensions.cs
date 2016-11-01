using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description;

namespace URSA.Web.Http.Configuration
{
    /// <summary>Provides useful <see cref="IComponentProvider" /> extensions.</summary>
    public static class ComponentProviderExtensions
    {
        /// <summary>Registers automatically discovered <see cref="IController" /> implementations.</summary>
        /// <param name="container">The container.</param>
        /// <param name="controllerDetailsAction">The controller details action.</param>
        /// <returns>The <paramref name="container" /> itself.</returns>
        public static IComponentProvider WithAutodiscoveredControllers(this IComponentProvider container, Action<Type, ControllerInfo> controllerDetailsAction = null)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var assemblies = UrsaConfigurationSection.GetInstallerAssemblies();
            container.RegisterAll<IController>(assemblies);
            var controllers = container.ResolveAllTypes<IController>();
            var registeredEntryPoints = new List<string>();
            foreach (var controller in controllers.Where(controller => !controller.IsDescriptionController()))
            {
                container.RegisterGenericControllerDescriptionBuilder(controller);
                container.RegisterApiDescriptionBuilder(controller);

                var descriptionBuilder = (IHttpControllerDescriptionBuilder)container.Resolve(typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(controller));
                var description = descriptionBuilder.BuildDescriptor();
                if ((description.EntryPoint != null) && (!registeredEntryPoints.Contains(description.EntryPoint.ToString())))
                {
                    container.RegisterEntryPointControllerDescriptionBuilder(description.EntryPoint.Url);
                    registeredEntryPoints.Add(description.EntryPoint.ToString());
                }

                if (controllerDetailsAction != null)
                {
                    controllerDetailsAction(controller, description);
                }
            }

            return container;
        }

        private static void RegisterApiDescriptionBuilder(this IComponentProvider container, Type controllerType)
        {
            var genericServiceType = typeof(IApiDescriptionBuilder<>).MakeGenericType(controllerType);
            var implementationType = typeof(ApiDescriptionBuilder<>).MakeGenericType(controllerType);
            container.Register(genericServiceType, implementationType, genericServiceType.FullName, lifestyle: Lifestyles.Singleton);
            container.Register(typeof(IApiDescriptionBuilder), implementationType, implementationType.FullName, lifestyle: Lifestyles.Singleton);
        }

        private static void RegisterGenericControllerDescriptionBuilder(this IComponentProvider container, Type controllerType)
        {
            var genericServiceType = typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(controllerType);
            var implementationType = typeof(ControllerDescriptionBuilder<>).MakeGenericType(controllerType);
            container.Register(genericServiceType, implementationType, genericServiceType.FullName, lifestyle: Lifestyles.Singleton);
            if (!typeof(EntryPointDescriptionController).IsAssignableFrom(controllerType))
            {
                container.Register(typeof(IHttpControllerDescriptionBuilder), implementationType, implementationType.FullName, lifestyle: Lifestyles.Singleton);
            }
        }

        private static void RegisterEntryPointControllerDescriptionBuilder(this IComponentProvider container, Url entryPoint)
        {
            container.Register<IHttpControllerDescriptionBuilder, EntryPointControllerDescriptionBuilder>(
                entryPoint.ToString().Substring(1),
                () => new EntryPointControllerDescriptionBuilder(entryPoint, container.Resolve<IDefaultValueRelationSelector>()),
                Lifestyles.Singleton);
        }

        private static bool IsDescriptionController(this Type controllerType)
        {
            return (typeof(EntryPointDescriptionController).IsAssignableFrom(controllerType)) ||
                ((controllerType.GetTypeInfo().IsGenericType) && (controllerType.GetGenericTypeDefinition() == typeof(DescriptionController<>)));
        }
    }
}