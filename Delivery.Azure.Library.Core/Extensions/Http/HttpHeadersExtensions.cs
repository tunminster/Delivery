using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Core.Extensions.Http
{
    public static class HttpHeadersExtensions
    {
        /// <summary>
        ///     List of sensitive headers we should never log
        /// </summary>
        public static readonly ReadOnlyCollection<string> SensitiveHeaders = new ReadOnlyCollection<string>(
            new List<string>
            {
                "ocp-apim-subscription-key",
                "subscription-key",
                "clientcert",
                "x-arr-ssl",
                "authorization"
            });

        /// <summary>
        ///     Returns a dictionary which represents all the headers describing the content
        /// </summary>
        /// <param name="httpHeaders">Content headers</param>
        public static Dictionary<string, string> AsDictionary(this System.Net.Http.Headers.HttpHeaders httpHeaders)
        {
            return httpHeaders.ToDictionary(header => header.Key, header => header.Value.Format());
        }
    }
}