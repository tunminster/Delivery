using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Connection.Managers;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Delivery.Azure.Library.Storage.Cosmos.Serializers;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Storage.Cosmos.Connections
{
    public class CosmosDatabaseConnectionManager : ConnectionManager<CosmosDatabaseConnection>, ICosmosDatabaseConnectionManager
    {
        public CosmosDatabaseConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override int PartitionCount => ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("CosmosDatabase_ConnectionManager_PartitionCount", defaultValue: 0);
        public override DependencyType DependencyType => DependencyType.Cosmos;
        public override ExternalDependency ExternalDependency => ExternalDependency.Cosmos;
        private int RequestTimeout => ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("CosmosDatabase_Request_Timeout", defaultValue: 10);
        
        protected override async Task<CosmosDatabaseConnection> CreateConnectionAsync(IConnectionMetadata connectionMetadata)
        {
            var connectionString = await GetConnectionStringAsync(connectionMetadata);
            var cosmosClient = new CosmosClientBuilder(connectionString)
                .WithConnectionModeDirect()
                .WithThrottlingRetryOptions(TimeSpan.FromSeconds(value: 30), maxRetryAttemptsOnThrottledRequests: 20)
                .WithRequestTimeout(TimeSpan.FromSeconds(RequestTimeout))
                .WithCustomSerializer(new CosmosTextJsonSerializer(JsonExtensions.GetDefaultJsonSerializerOptions()))
                .Build();

            return new CosmosDatabaseConnection(connectionMetadata, cosmosClient);
        }
        
        private async Task<string> GetConnectionStringAsync(IConnectionMetadata connectionMetadata)
        {
            var connectionString = await ServiceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync(connectionMetadata.SecretName);
            return connectionString;
        }
        
    }
}