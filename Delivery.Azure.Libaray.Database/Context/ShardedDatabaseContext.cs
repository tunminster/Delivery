using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Libaray.Database.Context.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Libaray.Database.Context
{
    /// <summary>
    ///  A database context which is aware of which shard to connect to
    /// </summary>
	public abstract class ShardedDatabaseContext : DbContext, IDisposableShardedDbContext, IShardAware
    {
        public IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; }
	    protected IServiceProvider ServiceProvider { get; }
        public IShard Shard => ExecutingRequestContextAdapter.GetShard();

        /// <summary>
        ///     For simplicity the whole database can be stored in memory and cleared when changed to avoid cache invalidation issues
        ///     with related entities which is a very complex area to get right
        /// </summary>
        public string GlobalDatabaseCacheRegion => $"Database-{Shard.Key}";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="executingRequestContextAdapter">Details of the call made to the database</param>
        /// <param name="dbContextOptions">
        ///     The result of running the <see cref="DbContextOptionsBuilder" /> and contains
        ///     database-specific connection logic
        /// </param>
        /// <remarks>
        ///     This constructor is required by <see cref="MigrationConfiguration{TDatabaseContext}" /> - do not remove
        ///     without changing <see cref="MigrationConfiguration{TDatabaseContext}" />
        /// </remarks>
        protected ShardedDatabaseContext(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
	        ExecutingRequestContextAdapter = executingRequestContextAdapter;
	        ServiceProvider = serviceProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurePolicies(modelBuilder);
            ConfigureIndexes(modelBuilder);
        }

        protected virtual void ConfigurePolicies(ModelBuilder modelBuilder)
        {
            // database has a soft delete concept so prevent accidental deletions from the code
            ConfigureSoftDeletePolicy(modelBuilder);

            // 7 decimal places is overkill for date precision
            ConfigureDateTimeOffsetPrecisionPolicy(modelBuilder);
        }

        private void ConfigureDateTimeOffsetPrecisionPolicy(ModelBuilder modelBuilder)
        {
            var mutableProperties = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(entityType => entityType.GetProperties());

            foreach (var property in mutableProperties.Where(mutableProperty => mutableProperty.ClrType == typeof(DateTimeOffset) || mutableProperty.ClrType == typeof(DateTimeOffset?)))
            {
                property.SetColumnType("datetimeoffset(3)");
            }
        }

        private void ConfigureSoftDeletePolicy(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(entityType => entityType.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        protected virtual void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // find entities with specific derived types where indexes need to be configured using reflection and the fluent api
            var entities = modelBuilder.Model.GetEntityTypes();

            var softDeleteEntities = entities.Where(entityType => entityType.ClrType.IsSubclassOf(typeof(SoftDeleteEntity))).ToList();
            foreach (var entity in softDeleteEntities)
            {
                var isDeletedColumn = nameof(SoftDeleteEntity.IsDeleted);
                var externalIdColumn = nameof(SoftDeleteEntity.ExternalId);

                modelBuilder.Entity(entity.ClrType).HasIndex(isDeletedColumn).HasName("IX_IsDeleted");
                modelBuilder.Entity(entity.ClrType).HasIndex(externalIdColumn).HasName("IX_ExternalId");
            }

            var softDeleteAuditedEntities = entities.Where(entityType => entityType.ClrType.IsSubclassOf(typeof(SoftDeleteAuditedEntity))).ToList();
            foreach (var entityType in softDeleteAuditedEntities)
            {
                var insertedByColumn = nameof(SoftDeleteAuditedEntity.InsertedBy);
                var insertionDateTimeColumn = nameof(SoftDeleteAuditedEntity.InsertionDateTime);

                modelBuilder.Entity(entityType.ClrType).HasIndex(insertedByColumn).HasName("IX_InsertedBy");
                modelBuilder.Entity(entityType.ClrType).HasIndex(insertionDateTimeColumn).HasName("IX_InsertionDateTime");
            }

            ConfigureExternalIdUniqueConstraint(modelBuilder);
        }

        protected virtual bool IsGenerateExternalIdUniqueConstraint(IMutableEntityType entityType)
        {
            return entityType.ClrType.IsSubclassOf(typeof(Entity));
        }

        private void ConfigureExternalIdUniqueConstraint(ModelBuilder modelBuilder)
        {
            var entities = modelBuilder.Model.GetEntityTypes().Where(IsGenerateExternalIdUniqueConstraint).ToList();
            foreach (var entity in entities)
            {
                var externalIdColumn = nameof(Entity.ExternalId);
                modelBuilder.Entity(entity.ClrType).HasIndex(externalIdColumn).IsUnique().HasName("IX_UniqueExternalId");
            }
        }

        public bool IsDisposed { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            MeasureConnectionClosed(Shard);

            IsDisposed = true;
        }

        private void MeasureConnectionClosed(IShard shard)
        {
            var customProperties = GetCustomProperties(shard);

            var telemetry = ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>();
            telemetry.TrackMetric(CustomMetrics.ClosedConnections, value: 1, customProperties);
        }

        private static Dictionary<string, string> GetCustomProperties(IShard shard)
        {
	        var customProperties = new Dictionary<string, string>
	        {
		        {CustomProperties.ConnectionType, "Database"},
		        {CustomProperties.Shard, shard.Key}
	        };
	        return customProperties;
        }

        public override int SaveChanges()
        {
	        throw new NotSupportedException("Use async methods only");
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
	        throw new NotSupportedException("Use async methods only");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
	        return await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
	        var featureProvider = ServiceProvider.GetRequiredService<IFeatureProvider>();
	        if (!await featureProvider.IsEnabledAsync(DatabaseFeatures.DatabaseSave.ToString()))
	        {
		        throw new FeatureDisabledException("Database save feature flag is disabled");
	        }

	        var saveChangeResult = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

	        await ClearCacheAsync();

	        return saveChangeResult;
        }

        private async Task ClearCacheAsync()
        {
	        var cache = await ServiceProvider.GetInvalidationEnabledCacheAsync();
	        if (cache != null)
	        {
		        try
		        {
			        await cache.ClearAsync(GlobalDatabaseCacheRegion, ExecutingRequestContextAdapter.GetCorrelationId());
		        }
		        catch (Exception exception)
		        {
			        // allow the live and audit records to be persisted even if redis is not reachable
			        var customProperties = GetCustomProperties(Shard);
			        ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(exception, customProperties);
		        }
	        }
        }

        public override async ValueTask DisposeAsync()
        {
	        var customProperties = GetCustomProperties(Shard);
	        ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric(CustomMetrics.ClosedConnections, value: 1, customProperties);
	        await base.DisposeAsync();
        }
    }
}