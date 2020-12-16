using System;
using System.Globalization;
using System.Linq;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Environments.Extensions;
using Delivery.Azure.Library.Messaging.ServiceBus.Properties;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.Microservices.Hosting.Extensions
{
    public static class PlatformEnvironmentExtensions
    {
        public static IHostBuilder ConfigurePlatformEnvironment(this IHostBuilder hostBuilder, params string[] args)
        {
            var environmentName = RuntimeEnvironmentExtensions.GetEnvironmentName();

            if (string.IsNullOrEmpty(environmentName) && args.Length > 0)
            {
                var environmentCommandLine = args.LastOrDefault(arg => arg.StartsWith("environment=", ignoreCase: true, CultureInfo.InvariantCulture));
                if (!string.IsNullOrEmpty(environmentCommandLine))
                {
                    environmentName = environmentCommandLine.Split(separator: '=')[1];
                }
            }

            hostBuilder.UseEnvironment(environmentName);
            return hostBuilder;
        }
        
        public static int GetRuntimeRing(this IServiceProvider serviceProvider)
        {
            var ring = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<int?>(MessageProperties.Ring, defaultValue: null);
            if (ring.HasValue)
            {
                return ring.GetValueOrDefault();
            }

            var useInMemory = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<bool>("Test_Use_In_Memory", defaultValue: false);

            // ensure that local development messages do not clash
            if (!useInMemory)
            {
                throw new InvalidOperationException("Expected a ring to be set as an environment variable, or for the subscriptions to be managed by the tests");
            }

            var buildId = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<string?>("System-JobId", defaultValue: null);
            if (string.IsNullOrEmpty(buildId))
            {
                return Environment.MachineName.Sum(c => (int) c * 20);
            }

            var jobIdRing = buildId.Sum(c => (int) c * 20);
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Using ring {jobIdRing} calculated from job id {buildId}");
            return jobIdRing;
        }
    }
}