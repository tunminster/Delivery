using System.Configuration;
using Delivery.Azure.Library.Caching.Cache;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Environments;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Configuration.Features;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.KeyVault.Providers;
using Delivery.Azure.Library.Messaging.HostedServices;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections.Interfaces;
using Delivery.Azure.Library.Microservices.Hosting.HostedServices;
using Delivery.Azure.Library.Resiliency.Stability;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Metadata;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Azure.Library.Storage.Cosmos.Connections;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Stdout;
using Delivery.Azure.Library.WebApi;
using Delivery.Store.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConfigurationProvider = Delivery.Azure.Library.Configuration.Configurations.ConfigurationProvider;
using IConfigurationProvider = Delivery.Azure.Library.Configuration.Configurations.Interfaces.IConfigurationProvider;

namespace Delivery.Orders.Host.Kernel
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
            serviceCollection.AddSingleton<IServiceBusSenderConnectionManager, ServiceBusSenderConnectionManager>();
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
                serviceCollection.AddPlatformRedisCache();
                serviceCollection.AddSingleton<IManagedCache, ManagedRedisCache>();
            }
            
            serviceCollection.AddSingleton<IServiceBusReceiverConnectionManager, ServiceBusReceiverConnectionManager>();
            serviceCollection.AddSingleton<ICosmosDatabaseConnectionManager, CosmosDatabaseConnectionManager>();
            
            serviceCollection.AddElasticSearch(configuration);
            
            serviceCollection.AddHostedService(serviceProvider => new MultipleTasksBackgroundService(
                new QueueServiceBusWorkBackgroundService(serviceProvider),
                //new QueueBlobUploadWorkBackgroundService(serviceProvider),
                new LifetimeEventsHostedService(serviceProvider, serviceProvider.GetRequiredService<IHostApplicationLifetime>())));

            return serviceCollection;

        }
    }
}