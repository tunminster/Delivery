using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Connection.Managers;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Connections
{
    public abstract class ServiceBusConnectionManager<TConnection> : ConnectionManager<TConnection>
		where TConnection : Connection.Managers.Connection
	{
		public override int PartitionCount => ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("ServiceBus_ConnectionManager_PartitionCount", defaultValue: 1);
		public override DependencyType DependencyType => DependencyType.Messaging;
		public override ExternalDependency ExternalDependency => ExternalDependency.ServiceBus;

		protected override async Task<TConnection> CreateConnectionAsync(IConnectionMetadata connectionMetadata)
		{
			TConnection connectionInformation;
			try
			{
				var circuitBreaker = ServiceProvider.GetRequiredService<ICircuitManager>().GetCircuitBreaker(DependencyType.Messaging, "ServiceBus");
				connectionInformation = await circuitBreaker.CommunicateAsync(async () => await AcquireConnectionAsync(connectionMetadata));
			}
			catch (UnauthorizedAccessException)
			{
				if (ServiceProvider.GetService<ISecretProvider?>() is ICachedSecretProvider cachedSecretProvider)
				{
					await cachedSecretProvider.ClearCacheAsync(connectionMetadata.SecretName);
				}

				connectionInformation = await AcquireConnectionAsync(connectionMetadata);
			}

			return connectionInformation;
		}

		private async Task<TConnection> AcquireConnectionAsync(IConnectionMetadata connectionMetadata)
		{
			var connectionString = await GetConnectionStringAsync(connectionMetadata);
			return CreateServiceBusConnection(connectionMetadata, connectionString);
		}

		private TConnection CreateServiceBusConnection(IConnectionMetadata connectionMetadata, string connectionString)
		{
			var queueOrTopicName = connectionMetadata.EntityName;

			connectionString = ReplaceEntityPath(connectionString);

			var messageSender = CreateConnection(connectionMetadata, connectionString, queueOrTopicName);
			return messageSender;
		}

		public static string ReplaceEntityPath(string connectionString)
		{
			var entityPath = "EntityPath=";
			if (connectionString.Contains(entityPath))
			{
				connectionString = connectionString.Split(entityPath)[0];
			}

			return connectionString;
		}

		protected abstract TConnection CreateConnection(IConnectionMetadata connectionMetadata, string connectionString, string queueOrTopicName);

		private async Task<string> GetConnectionStringAsync(IConnectionMetadata connectionMetadata)
		{
			var connectionString = await ServiceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync(connectionMetadata.SecretName);

			return connectionString;
		}

		protected ServiceBusConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
	}
}