namespace URSA.ComponentModel
{
    /// <summary>Defines a component installer.</summary>
    public interface IComponentInstaller
    {
        /// <summary>Installs additional components.</summary>
        /// <param name="componentProviderBuilder">Component provider builder to be used for registrations.</param>
        /// <param name="componentProvider">Component provider to be used for resolving existing registrations.</param>
        void InstallComponents(IComponentProviderBuilder componentProviderBuilder, IComponentProvider componentProvider);
    }
}