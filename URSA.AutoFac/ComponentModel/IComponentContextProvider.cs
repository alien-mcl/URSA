using Autofac;

namespace URSA.AutoFac.ComponentModel
{
    internal interface IComponentContextProvider
    {
        IComponentContextProvider Parent { get; }

        IComponentContext Container { get; set; }
    }
}