using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.Core.Extensions.Http
{
    public static class HeaderDictionaryExtensions
    {
        /// <summary>
        ///     Returns a dictionary which represents all the headers describing the content
        /// </summary>
        /// <param name="httpHeaders">Content headers</param>
        public static Dictionary<string, string> AsDictionary(this IHeaderDictionary httpHeaders)
        {
            return httpHeaders.ToDictionary(header => header.Key, header => header.Value.Format());
        }
    }
}