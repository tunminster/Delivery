using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Delivery.Azure.Library.WebApi.Conventions
{
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?tabs=aspnetcore2x&view=aspnetcore-2.1#how-to-use-kestrel-in-aspnet-core-apps
    public static class PlatformKestrelExtensions
    {
        // ReSharper disable once InvalidXmlDocComment
        /// <summary>
        ///     Sets the standard platform rate limits and kestrel options
        /// </summary>
        public static KestrelServerOptions UsePlatformKestrelServerOptions(this KestrelServerOptions kestrelServerOptions)
        {
            kestrelServerOptions.AddServerHeader = false;

            // enable http2
            kestrelServerOptions.ConfigureEndpointDefaults(options => options.Protocols = HttpProtocols.Http1AndHttp2);

            kestrelServerOptions.Limits.MaxRequestBodySize = 62914560;
            return kestrelServerOptions;
        }
    }
}