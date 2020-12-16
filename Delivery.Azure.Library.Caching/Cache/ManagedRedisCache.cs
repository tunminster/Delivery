using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Configurations;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Core.Monads;
using Delivery.Azure.Library.Exceptions.Writers;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Azure.Library.Core.Extensions.Json;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Delivery.Azure.Library.Caching.Cache
{
    /// <summary>
    ///     Based on the StackExchange redis library to store objects as json strings using the default
    ///     <see cref="JsonExtensions" />
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
		private readonly IRedisCacheClient redisCacheClient;

		private IApplicationInsightsTelemetry Telemetry => serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		public ManagedRedisCache(IServiceProvider serviceProvider) : this(serviceProvider, new RedisCacheConfigurationDefinition(serviceProvider))
		{
		}

		public ManagedRedisCache(IServiceProvider serviceProvider, RedisCacheConfigurationDefinition configurationDefinition)
		{
			this.serviceProvider = serviceProvider;
			this.configurationDefinition = configurationDefinition;
			redisCacheClient = serviceProvider.GetRequiredService<IRedisCacheClient>();
		}

		public virtual async Task<Maybe<T>> AddAsync<T>(string key, T targetValue, string partition, string correlationId, int? cacheExpirySeconds = null)
		{
			try
			{
				ValidatePartition(partition, key);

				var expiry = TimeSpan.FromSeconds(cacheExpirySeconds.GetValueOrDefault(configurationDefinition.DefaultCacheExpirySeconds));
				var dependencyName = redisCacheClient.GetDbFromConfiguration().Database.Database.ToString();
				var dependencyTarget = redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetEndPoints().OfType<DnsEndPoint>().FirstOrDefault()?.Host ?? redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetEndPoints().OfType<IPEndPoint>().FirstOrDefault()?.Address?.ToString() ?? "Unknown";
				var dependencyData = new DependencyData("Add", targetValue);

				await new DependencyMeasurement(serviceProvider)
					.ForDependency(dependencyName, MeasuredDependencyType.Redis, dependencyData.ConvertToJson(), dependencyTarget)
					.WithCorrelationId(correlationId)
					.TrackAsync(async () =>
					{
						return await Policy
							.Handle<InvalidOperationException>()
							.Or<InvalidOperationException>()
							.WaitAndRetryAsync(retryCount: 60, retryAttempt => TimeSpan.FromMilliseconds(value: 500))
							.ExecuteAsync(async () => await redisCacheClient.GetDbFromConfiguration().AddAsync(key, targetValue, expiry));
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
					await Policy
						.Handle<InvalidOperationException>()
						.Or<InvalidOperationException>()
						.WaitAndRetryAsync(retryCount: 60, retryAttempt => TimeSpan.FromMilliseconds(value: 500))
						.ExecuteAsync(async () =>
						{
							var connectionMultiplexer = redisCacheClient.GetDbFromConfiguration().Database.Multiplexer;
							var endpoints = connectionMultiplexer.GetEndPoints();
							foreach (var endpoint in endpoints)
							{
								var server = connectionMultiplexer.GetServer(endpoint);
								var keys = server.KeysAsync(pattern: partition + "*");
								await foreach (var redisKey in keys)
								{
									await redisCacheClient.GetDbFromConfiguration().Database.KeyDeleteAsync(redisKey);
								}
							}
						});
				}
			}
			catch (TimeoutException exception)
			{
				Telemetry.TrackTrace(exception.WriteException($"Timeout during clear redis partition {partition}"), SeverityLevel.Error);
			}
		}

		public ValueTask DisposeAsync()
		{
			try
			{
				redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.Dispose();
			}
			catch (Exception)
			{
				// dispose is best-effort
			}

			return new ValueTask();
		}

		public virtual async Task<Maybe<T>> GetAsync<T>(string key, string partition)
		{
			try
			{
				ValidatePartition(partition, key);

				var dependencyName = redisCacheClient.GetDbFromConfiguration().Database.Database.ToString();
				var dependencyTarget = redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetEndPoints().OfType<DnsEndPoint>().FirstOrDefault()?.Host ?? redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetEndPoints().OfType<IPEndPoint>().FirstOrDefault()?.Address?.ToString() ?? "Unknown";
				var dependencyData = new DependencyData("Get", key);

				var redisValue = await new DependencyMeasurement(serviceProvider)
					.ForDependency(dependencyName, MeasuredDependencyType.Redis, dependencyData.ConvertToJson(), dependencyTarget)
					.TrackAsync(async () =>
					{
						return await Policy
							.Handle<InvalidOperationException>()
							.Or<InvalidOperationException>()
							.WaitAndRetryAsync(retryCount: 60, retryAttempt => TimeSpan.FromMilliseconds(value: 500))
							.ExecuteAsync(async () => await redisCacheClient.GetDbFromConfiguration().GetAsync<T>(key));
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
    }
}