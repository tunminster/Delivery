using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Exceptions;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Connection.Managers
{
    public abstract class ConnectionManager<TConnection> : IConnectionManager<TConnection>, IAsyncDisposable where TConnection : IConnection
    {
        protected IServiceProvider ServiceProvider { get; }
        private IApplicationInsightsTelemetry Telemetry => ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

        private readonly ConcurrentDictionary<string, TConnection> connections = new();

        private readonly Random random = new();
        
        public abstract int PartitionCount { get; }
        public abstract DependencyType DependencyType { get; }
        public abstract ExternalDependency ExternalDependency { get; }

        protected ConnectionManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        
        public async Task<TConnection> GetConnectionAsync(string entityName, string secretName, int? partition = null)
        {
            if (!partition.HasValue)
            {
                partition = random.Next(minValue: 0, PartitionCount);
            }

            ValidatePartition(partition);

            var connectionMetadata = new ConnectionMetadata(entityName, secretName, partition.GetValueOrDefault());
            var connection = await GetConnectionAsync(connectionMetadata);

            return connection;
        }

        private async Task<TConnection> GetConnectionAsync(IConnectionMetadata connectionMetadata)
		{
			var connectionCacheKey = ComposeUniqueKey(connectionMetadata);

			if (!connections.TryGetValue(connectionCacheKey, out var connection))
			{
				try
				{
					var circuitBreaker = ServiceProvider.GetRequiredService<ICircuitManager>().GetCircuitBreaker(DependencyType, ExternalDependency.ToString());
					connection = await circuitBreaker.CommunicateAsync(async () => await CreateConnectionAsync(connectionMetadata));
				}
				catch (Exception exception)
				{
					throw new ConnectionFailedException(connectionMetadata.EntityName, GetType().Name, connectionMetadata.SecretName, exception);
				}

				connections.AddOrUpdate(connectionCacheKey, connection, (currentKey, oldConnection) => connection);

				MeasureNewConnectionCreated(connectionMetadata, connectionCacheKey);
			}

			return connection;
		}

		private void MeasureNewConnectionCreated(IConnectionMetadata connectionMetadata, string connectionCacheKey)
		{
			var customProperties = GetConnectionMeasurementProperties(connectionMetadata, connectionCacheKey);
			Telemetry.TrackMetric(CustomMetrics.CreatedConnections, value: 1, customProperties);
		}

		private void MeasureConnectionClosed(IConnectionMetadata connectionMetadata, string connectionCacheKey)
		{
			var customProperties = GetConnectionMeasurementProperties(connectionMetadata, connectionCacheKey);
			Telemetry.TrackMetric(CustomMetrics.ClosedConnections, value: 1, customProperties);
		}

		private Dictionary<string, string> GetConnectionMeasurementProperties(IConnectionMetadata connectionMetadata, string connectionCacheKey)
		{
			var customMetricProperties = new Dictionary<string, string>
			{
				{CustomProperties.EntityName, connectionMetadata.EntityName},
				{CustomProperties.SecretName, connectionMetadata.SecretName},
				{CustomProperties.CacheKey, connectionCacheKey},
				{CustomProperties.ConnectionCacheKey, connectionCacheKey},
				{CustomProperties.ConnectionMetadata, connectionMetadata.Format()},
				{CustomProperties.ConnectionType, GetType().Name}
			};

			return customMetricProperties;
		}

		private static string ComposeUniqueKey(IConnectionMetadata connectionMetadata)
		{
			return $"{connectionMetadata.SecretName}-{connectionMetadata.EntityName}-{connectionMetadata.Partition}";
		}

		public virtual async Task ReleaseConnectionAsync(IConnectionMetadata connectionMetadata)
		{
			var uniqueKey = ComposeUniqueKey(connectionMetadata);

			if (connections.TryRemove(uniqueKey, out var connection))
			{
				await connection.DisposeAsync();
				MeasureConnectionClosed(connectionMetadata, uniqueKey);
			}

			var customProperties = GetConnectionMeasurementProperties(connectionMetadata, uniqueKey);
			Telemetry.TrackTrace($"Connection for '{connectionMetadata.EntityName}' with connection string name '{connectionMetadata.SecretName}' was released.", SeverityLevel.Warning, customProperties);
		}

		public virtual async Task<TConnection> RenewConnectionAsync(IConnectionMetadata connectionMetadata)
		{
			await ReleaseConnectionAsync(connectionMetadata);
			var newConnection = await GetConnectionAsync(connectionMetadata.EntityName, connectionMetadata.SecretName, connectionMetadata.Partition);

			return newConnection;
		}

		protected abstract Task<TConnection> CreateConnectionAsync(IConnectionMetadata connectionMetadata);

		protected void ValidatePartition(int? partition)
		{
			if (!partition.HasValue || partition < 0 || partition > PartitionCount)
			{
				throw new InvalidOperationException($"Specific partition must be within the acceptable range {default(int)}-{PartitionCount}. Found: {partition}");
			}
		}

		public async ValueTask DisposeAsync()
		{
			foreach (var connection in connections.Values.OfType<IAsyncDisposable>())
			{
				await connection.DisposeAsync();
			}
		}
    }
}