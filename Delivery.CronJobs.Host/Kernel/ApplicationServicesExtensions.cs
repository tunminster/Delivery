using Delivery.Azure.Library.Caching.Cache;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Environments;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Configuration.Features;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.KeyVault.Providers;
using Delivery.Azure.Library.NotificationHub.Connections;
using Delivery.Azure.Library.NotificationHub.Connections.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Metadata;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Stdout;
using Delivery.Azure.Library.WebApi;
using Delivery.Store.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;

namespace Delivery.CronJobs.Host.Kernel
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddDefaultHttpClient();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
            serviceCollection.AddSingleton<IEnvironmentProvider, EnvironmentProvider>();
            serviceCollection.AddSingleton<ISecretProvider, KeyVaultCachedSecretProvider>();
            
            
#if DEBUG
            // log the local debug information to the output window for easier testing
            serviceCollection.AddSingleton<IApplicationInsightsTelemetry, StdoutApplicationInsightsTelemetry>(provider => new StdoutApplicationInsightsTelemetry(provider, configuration.GetValue<string>("Service_Name")));
#else
			serviceCollection.AddSingleton<IApplicationInsightsTelemetry, Delivery.Azure.Library.Telemetry.ApplicationInsights.ApplicationInsightsTelemetry>(provider => new Delivery.Azure.Library.Telemetry.ApplicationInsights.ApplicationInsightsTelemetry(provider, configuration.GetValue<string>("Service_Name"), new Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers.ApplicationTelemetryInitializers(provider, typeof(ApplicationServicesExtensions).Assembly)));
#endif
            serviceCollection.AddSingleton<IShardMetadataManager, ShardMetadataManager>();
            serviceCollection.AddSingleton<IShardDatabaseManager, ShardDatabaseManager>();
            serviceCollection.AddSingleton<IFeatureProvider, FeatureProvider>();
            serviceCollection.AddSingleton<ICircuitManager, CircuitManager>();
            
            var useInMemory = configuration.GetValue<bool?>("Test_Use_In_Memory");
            if (useInMemory.GetValueOrDefault())
            {
                serviceCollection.AddSingleton<IManagedCache, ManagedMemoryCache>();
            }
            else
            {
                serviceCollection.AddPlatformCaching();
                serviceCollection.AddSingleton<IManagedCache, ManagedRedisCache>();
            }
            
            serviceCollection.AddSingleton<INotificationHubSenderConnectionManager, NotificationHubSenderConnectionManager>();
            serviceCollection.AddElasticSearch(configuration);
            
            return serviceCollection;

        }
    }
}