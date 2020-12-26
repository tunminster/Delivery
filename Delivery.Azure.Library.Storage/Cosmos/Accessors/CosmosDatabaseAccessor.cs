using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Connections;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Storage.Cosmos.Accessors
{
    public class CosmosDatabaseAccessor
    {
        private static readonly ConcurrentDictionary<string, Container> cachedContainers = new();

		protected CosmosDatabaseAccessor(CosmosDatabaseConnection storageConnection, CosmosDatabaseConnectionConfigurationDefinition configurationDefinition)
		{
			CosmosDatabaseConnection = storageConnection;
			ConfigurationDefinition = configurationDefinition;
		}

		/// <summary>
		///     Connection to the cosmos database
		/// </summary>
		protected CosmosDatabaseConnection CosmosDatabaseConnection { get; }

		/// <summary>
		///     Configuration definition of the cosmos connection
		/// </summary>
		protected CosmosDatabaseConnectionConfigurationDefinition ConfigurationDefinition { get; }

		/// <summary>
		///     Creates a new instance of the cosmos database accessor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="shard">
		///     The shard to connect to using the secret name convention in
		///     <see cref="CosmosDatabaseConnectionConfigurationDefinition" />
		/// </param>
		public static async Task<CosmosDatabaseAccessor> CreateAsync(IServiceProvider serviceProvider, IShard shard)
		{
			var configurationDefinition = new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider, shard);

			return await CreateAsync(serviceProvider, configurationDefinition);
		}

		/// <summary>
		///     Creates a new instance of the cosmos database accessor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="configurationDefinition">Configuration of the cosmos database</param>
		public static async Task<CosmosDatabaseAccessor> CreateAsync(IServiceProvider serviceProvider, CosmosDatabaseConnectionConfigurationDefinition? configurationDefinition = null)
		{
			configurationDefinition ??= new CosmosDatabaseConnectionConfigurationDefinition(serviceProvider);
			var connection = await GetConnectionTaskAsync(serviceProvider, configurationDefinition);

			return new CosmosDatabaseAccessor(connection, configurationDefinition);
		}

		public CosmosClient CosmosClient => CosmosDatabaseConnection.CosmosClient;

		public Container GetContainer(string databaseName, string containerName)
		{
			cachedContainers.TryGetValue(containerName, out var cachedContainer);
			if (cachedContainer != null)
			{
				return cachedContainer;
			}

			var container = CosmosClient.GetContainer(databaseName, containerName);
			cachedContainers.TryAdd(containerName, container);
			return container;
		}

		protected static Task<CosmosDatabaseConnection> GetConnectionTaskAsync(IServiceProvider serviceProvider, CosmosDatabaseConnectionConfigurationDefinition configurationDefinition)
		{
			var connectionManager = serviceProvider.GetRequiredService<ICosmosDatabaseConnectionManager>();
			var connection = connectionManager.GetConnectionAsync(configurationDefinition.SecretName, configurationDefinition.SecretName);
			return connection;
		}
    }
}