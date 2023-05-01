using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Database.Factories
{
    public static class DatabaseContextFactory
	{
		/// <summary>
		///     Creates a new instance using <see cref="ISecretProvider" /> as connection string store
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="executingRequestContextAdapter">Contains details about the current request context</param>
		/// <param name="factory">A function that creates a new database context</param>
		public static async Task<TDatabaseContext> CreateAsync<TDatabaseContext>(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, Func<IExecutingRequestContextAdapter, DbContextOptions, TDatabaseContext> factory)
		{
			var databaseShardManager = new ShardDatabaseManager(serviceProvider);
			var connectionString = await databaseShardManager.GetConnectionStringAsync(executingRequestContextAdapter.GetShard());
			var dbContextOptions = new DbContextOptionsBuilder()
				.UseSqlServer(connectionString, optionsBuilder => ConfigureOptionsBuilder(serviceProvider, optionsBuilder)).Options;
			var sampleDatabaseContext = factory.Invoke(executingRequestContextAdapter, dbContextOptions);

			MeasureNewConnectionCreated(serviceProvider, executingRequestContextAdapter.GetShard());

			return sampleDatabaseContext;
		}
		

		/// <summary>
		///     Configures the sql database connection options builder
		/// </summary>
		/// <param name="serviceProvider">The kernel to use</param>
		/// <param name="optionsBuilder">The entity-framework provided builder</param>
		private static void ConfigureOptionsBuilder(IServiceProvider serviceProvider, SqlServerDbContextOptionsBuilder optionsBuilder)
		{
			var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
			var commandTimeout = configurationProvider.GetSettingOrDefault("Sql_CommandTimeoutSeconds", 60 * 2);
			var maxRetryCount = configurationProvider.GetSettingOrDefault("Sql_MaxRetryCount", defaultValue: 10);
			var maxRetryDelay = configurationProvider.GetSettingOrDefault("Sql_RetryDelaySeconds", defaultValue: 5);

			optionsBuilder
				.CommandTimeout(commandTimeout)
				.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelay), new List<int>());
		}

		private static void MeasureNewConnectionCreated(IServiceProvider serviceProvider, IShard shard)
		{
			var customProperties = new Dictionary<string, string>
			{
				{CustomProperties.ConnectionType, "Database"},
				{CustomProperties.Shard, shard.Key}
			};

			var telemetry = serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();
			telemetry.TrackMetric(CustomMetrics.CreatedConnections, value: 1, customProperties);
		}
	}
}