using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Delivery.Azure.Library.WebApi
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddDefaultHttpClient(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddHttpClient(Options.DefaultName)
                .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                {
                    var handler = new HttpClientHandler();

                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }

                    return handler;
                });

            return serviceCollection;
        }
    }
}