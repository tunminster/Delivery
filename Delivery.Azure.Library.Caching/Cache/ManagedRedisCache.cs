using System;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Configurations;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Monads;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Azure.Library.Core.Extensions.Json;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.Redis;

namespace Delivery.Azure.Library.Caching.Cache
{
    /// <summary>
    ///     Based on the StackExchange redis library to store objects as json strings using the default
    ///     <see cref="ServiceStack.Text.JsonExtensions" />
    ///     Dependencies:
    ///     <see cref="IConfigurationProvider" />
    ///     <see cref="IApplicationInsightsTelemetry" />
    ///     Settings:
    ///     <see cref="RedisCacheConfigurationDefinition" />
    /// </summary>
    public class ManagedRedisCache : IManagedCache
    {
        private readonly RedisCacheConfigurationDefinition configurationDefinition;
		private readonly IServiceProvider serviceProvider;
		private readonly CancellationTokenSource disposingCancellationTokenSource = new();
		
		private readonly IRedisClientsManagerAsync redisClientsManager;

		private IApplicationInsightsTelemetry Telemetry => serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		public ManagedRedisCache(IServiceProvider serviceProvider) : this(serviceProvider, new RedisCacheConfigurationDefinition(serviceProvider))
		{
		}

		public ManagedRedisCache(IServiceProvider serviceProvider, RedisCacheConfigurationDefinition configurationDefinition)
		{
			this.serviceProvider = serviceProvider;
			this.configurationDefinition = configurationDefinition;
			var license = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting("ServiceStack-Redis-LicenseKey");
			Licensing.RegisterLicense(license);
			redisClientsManager = serviceProvider.GetRequiredService<IRedisClientsManagerAsync>();
		}

		public virtual async Task<Maybe<T>> AddAsync<T>(string key, T targetValue, string partition, string correlationId, int? cacheExpirySeconds = null)
		{
			try
			{
				ValidatePartition(partition, key);
				
				var expiry = TimeSpan.FromSeconds(cacheExpirySeconds.GetValueOrDefault(configurationDefinition.DefaultCacheExpirySeconds));
				var dependencyData = new DependencyData("Add", targetValue);
				await using var redisClient = await redisClientsManager.GetClientAsync(disposingCancellationTokenSource.Token);

				await new DependencyMeasurement(serviceProvider)
					.ForDependency($"Database-{redisClient.Db}", MeasuredDependencyType.Redis, dependencyData.ConvertToJson(), redisClient.Host)
					.WithCorrelationId(correlationId)
					.TrackAsync(async () =>
					{
						EnsureNotDisposing();
						return await redisClient.SetAsync(key, targetValue, expiry, disposingCancellationTokenSource.Token);
					});

				return new Maybe<T>(targetValue);
			}
			catch (TimeoutException exception)
			{
				Telemetry.TrackTrace(exception.WriteException("Timeout trying to write to redis cache"), SeverityLevel.Error);
				return Maybe<T>.NotPresent;
			}
		}

		public virtual async Task ClearAsync(string partition, string correlationId, string? key = null)
		{
			try
			{
				ValidatePartition(partition, key);

				if (!string.IsNullOrWhiteSpace(key))
				{
					await AddAsync(key, string.Empty, partition, correlationId);
				}
				else
				{
					await using var redisClient = await redisClientsManager.GetClientAsync(disposingCancellationTokenSource.Token);
					await redisClient.RemoveByPatternAsync(partition + "*", disposingCancellationTokenSource.Token);
				}
			}
			catch (TimeoutException exception)
			{
				Telemetry.TrackTrace(exception.WriteException($"Timeout during clear redis partition {partition}"), SeverityLevel.Error);
			}
		}

		public async ValueTask DisposeAsync()
		{
			disposingCancellationTokenSource.Cancel();
			try
			{
				await using var redisClient = await redisClientsManager.GetClientAsync();
				await redisClient.DisposeAsync();
				await Task.CompletedTask;
			}
			catch (Exception)
			{
				// dispose is best-effort
			}
		}

		public virtual async Task<Maybe<T>> GetAsync<T>(string key, string partition)
		{
			try
			{
				ValidatePartition(partition, key);

				await using var redisClient = await redisClientsManager.GetReadOnlyClientAsync(disposingCancellationTokenSource.Token);
				var dependencyData = new DependencyData("Get", key);

				var redisValue = await new DependencyMeasurement(serviceProvider)
					.ForDependency($"Database-{redisClient.Db}", MeasuredDependencyType.Redis, dependencyData.ConvertToJson(), redisClient.Host)
					.TrackAsync(async () =>
					{
						EnsureNotDisposing();
						var result = await redisClient.GetAsync<T>(key, disposingCancellationTokenSource.Token);
						return result;
					});

				return new Maybe<T>(redisValue);
			}
			catch (TimeoutException exception)
			{
				Telemetry.TrackTrace(exception.WriteException("Timeout trying to read from redis cache"), SeverityLevel.Error);
				return Maybe<T>.NotPresent;
			}
		}

		private static void ValidatePartition(string partition, string? key)
		{
			if (!string.IsNullOrEmpty(key) && !key.StartsWith(partition))
			{
				throw new InvalidOperationException($"The partition is a wildcard filter used to clear multiple keys, therefore the key must start with the partition name. Partition: {partition}, key: {key}");
			}
		}
		
		private void EnsureNotDisposing()
		{
			if (disposingCancellationTokenSource.Token.IsCancellationRequested)
			{
				throw new OperationCanceledException("A cache operation was attempted during cache disposal");
			}
		}
    }
}