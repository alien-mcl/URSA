namespace URSA.Web.Mapping
{
    /// <summary>Provides a basic contract for argument binders.</summary>
    public interface IArgumentBinder
    {
        /// <summary>Binds the arguments required to call the <paramref name="requestMapping" />.</summary>
        /// <param name="request">Request details.</param>
        /// <param name="requestMapping">Request mapping.</param>
        /// <returns>Instances of values to be used as arguments of the <paramref name="requestMapping" />.</returns>
        object[] BindArguments(IRequestInfo request, IRequestMapping requestMapping);
    }

    /// <summary>Provides a basic contract for argument binders.</summary>
    /// <typeparam name="T">Type of the request handled.</typeparam>
    public interface IArgumentBinder<T> : IArgumentBinder where T : IRequestInfo
    {
        /// <summary>Binds the arguments required to call the <paramref name="requestMapping" />.</summary>
        /// <param name="request">Request details.</param>
        /// <param name="requestMapping">Request mapping.</param>
        /// <returns>Instances of values to be used as arguments of the <paramref name="requestMapping" />.</returns>
        object[] BindArguments(T request, IRequestMapping requestMapping);
    }
}