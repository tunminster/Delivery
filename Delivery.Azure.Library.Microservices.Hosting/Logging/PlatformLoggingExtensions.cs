using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Delivery.Azure.Library.Microservices.Hosting.Logging
{
    public static class PlatformLoggingExtensions
    {
        public static IHostBuilder ConfigurePlatformLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                });
        }
    }
}