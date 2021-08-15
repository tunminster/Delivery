using System.Net;
using Microsoft.AspNetCore.Builder;

namespace Delivery.Azure.Library.WebApi.Conventions
{
    public static class PlatformServicePointExtensions
    {
        /// <summary>
        ///     Configures the default <see cref="ServicePointManager" /> settings
        ///     Dependencies:
        ///     [None]
        ///     Settings:
        ///     [None]
        /// </summary>
        public static IApplicationBuilder UsePlatformServicePointSettings(IApplicationBuilder applicationBuilder)
        {
            // see https://blogs.msdn.microsoft.com/windowsazurestorage/2010/06/25/nagles-algorithm-is-not-friendly-towards-small-requests/
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            return applicationBuilder;
        }
    }
}