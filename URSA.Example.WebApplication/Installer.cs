using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using RDeF.Entities;
using URSA.Example.WebApplication.Data;

namespace URSA.Example.WebApplication
{
    /// <summary>Installs HTTP components.</summary>
    public class Installer : IWindsorInstaller
    {
        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            InstallRdfDependencies(container);
            var storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data");
            var jsonFileRepository = new JsonFilePersistingRepository<Person, Guid>(storagePath);
            container.Register(Component.For<IPersistingRepository<Person, Guid>>().Instance(jsonFileRepository).Named("PersonsJsonFileRepository"));
        }

        private void InstallRdfDependencies(IWindsorContainer container)
        {
            container.Register(Component.For<IEntityContextFactory>().UsingFactoryMethod(kernel => EntityContextFactory.FromConfiguration("file")).LifestyleSingleton());
            container.Register(Component.For<IEntityContext>().UsingFactoryMethod(kernel => kernel.Resolve<IEntityContextFactory>().Create()).LifestyleScoped());
        }
    }
}