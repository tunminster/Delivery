using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Delivery.Azure.Library.Database.Context
{
    public abstract class MeasurableShardedDatabaseContext : ShardedDatabaseContext
	{
		protected MeasurableShardedDatabaseContext(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, DbContextOptions dbContextOptions) : base(serviceProvider, executingRequestContextAdapter, dbContextOptions)
		{
		}

		public override int SaveChanges()
		{
			return SaveChanges(acceptAllChangesOnSuccess: true);
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			throw new NotSupportedException("Use async methods only").WithTelemetry(ExecutingRequestContextAdapter.GetTelemetryProperties());
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			return await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
		}

		public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
		{
			var dependencyName = $"Database-{Shard.Key}";
			var dependencyData = new DependencyData("Live");
			var dependencyTarget = Shard.Key;

			var result = await new DependencyMeasurement(ServiceProvider)
				.ForDependency(dependencyName, MeasuredDependencyType.Sql, dependencyData.ConvertToJson(), dependencyTarget)
				.WithContextualInformation(ExecutingRequestContextAdapter.GetTelemetryProperties())
				.TrackAsync(async () => await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));

			return result;
		}

		internal async Task<int> SaveUntrackedChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			return await base.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
		}
		
	}
}