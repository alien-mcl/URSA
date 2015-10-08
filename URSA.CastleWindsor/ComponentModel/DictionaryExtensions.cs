using System.Collections;
using System.Collections.Generic;
using Castle.MicroKernel;

namespace URSA.CastleWindsor.ComponentModel
{
    internal static class DictionaryExtensions
    {
        internal static Arguments ToArguments(this IDictionary<string, object> arguments)
        {
            if (arguments == null)
            {
                return new Arguments();
            }

            var result = new Hashtable();
            foreach (var argument in arguments)
            {
                result.Add(argument.Key, argument.Value);
            }

            return new Arguments(result);
        }
    }
}