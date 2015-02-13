using System;
using System.Collections.Generic;
using URSA.Web.Mapping;

namespace URSA.Web.Tests
{
    [DependentRoute]
    public class AnotherTestController<T> : IController where T : IController
    {
        public IResponseInfo Response { get; set; }

        [Route("/")]
        public string Some()
        {
            return null;
        }
    }
}