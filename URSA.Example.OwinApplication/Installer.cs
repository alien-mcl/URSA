using System;
using System.IO;
using Autofac;
using RDeF.Entities;
using URSA.Example.WebApplication.Data;
using Module = Autofac.Module;

namespace URSA.Example.OwinApplication
{
    /// <summary>Installs HTTP components.</summary>
    public class Installer : Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            InstallRdfDependencies(builder);
#if CORE
            var storagePath = Path.Combine(Assembly.GetEntryAssembly().Location, "..", "App_Data");
#else
            var storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data");
#endif
            var jsonFileRepository = new JsonFilePersistingRepository<Person, Guid>(storagePath);
            builder.RegisterInstance(jsonFileRepository).Named<IPersistingRepository<Person, Guid>>("PersonsJsonFileRepository");
        }

        private void InstallRdfDependencies(ContainerBuilder builder)
        {
            builder.Register(context => EntityContextFactory.FromConfiguration("file")).As<IEntityContextFactory>().SingleInstance();
            builder.Register(context => context.Resolve<IEntityContextFactory>().Create()).As<IEntityContext>().InstancePerLifetimeScope();
        }
    }
}