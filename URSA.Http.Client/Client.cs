using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Http;

namespace URSA.Web.Http
{
    public class Client
    {
        public Client(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            BaseUri = baseUri;
        }

        private Uri BaseUri { get; set; }

        protected object Call(Verb verb, string url, dynamic uriArguments, params object[] bodyArguments)
        {
            return null;
        }
    }
}