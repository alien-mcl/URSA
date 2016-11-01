namespace URSA.ComponentModel
{
    /// <summary>Defines a component installer.</summary>
    public interface IComponentInstaller
    {
        /// <summary>Installs additional components.</summary>
        /// <param name="componentComposer">Component composer to be used for registrations.</param>
        void InstallComponents(IComponentComposer componentComposer);
    }
}