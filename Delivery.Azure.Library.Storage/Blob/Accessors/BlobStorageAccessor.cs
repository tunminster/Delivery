using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Blob.Configurations;
using Delivery.Azure.Library.Storage.Blob.Connections;
using Delivery.Azure.Library.Storage.Blob.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Storage.Blob.Accessors
{
    public class BlobStorageAccessor
    {
        protected BlobStorageAccessor(IExecutingRequestContextAdapter executingRequestContextAdapter, BlobStorageConnection storageConnection, BlobStorageConnectionConfigurationDefinition configurationDefinition)
        {
            BlobStorageConnection = storageConnection;
            ConfigurationDefinition = configurationDefinition;
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
        }

        protected IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; }
        
        /// <summary>
        ///     Connection to the blob storage
        /// </summary>
        protected BlobStorageConnection BlobStorageConnection { get; }

        /// <summary>
        ///     Configuration definition of the blob storage connection
        /// </summary>
        protected BlobStorageConnectionConfigurationDefinition ConfigurationDefinition { get; }
        
        /// <summary>
        ///     Creates a new instance of the blob storage accessor
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="executingRequestContextAdapter">Contains details about the request</param>
        /// <param name="configurationDefinition">Configuration of the blob storage connection</param>
        public static async Task<BlobStorageAccessor> CreateAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, BlobStorageConnectionConfigurationDefinition configurationDefinition)
        {
            var connection = await GetConnectionTaskAsync(serviceProvider, configurationDefinition);

            return new BlobStorageAccessor(executingRequestContextAdapter, connection, configurationDefinition);
        }
        
        protected static Task<BlobStorageConnection> GetConnectionTaskAsync(IServiceProvider serviceProvider, BlobStorageConnectionConfigurationDefinition configurationDefinition)
        {
            var connectionManager = serviceProvider.GetRequiredService<IBlobStorageConnectionManager>();
            var connection = connectionManager.GetConnectionAsync(configurationDefinition.ContainerName, configurationDefinition.SecretName);
            return connection;
        }
        
    }
}