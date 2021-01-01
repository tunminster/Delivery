using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Database.DataAccess.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Policies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;

namespace Delivery.Azure.Library.Database.DataAccess
{
    /// <summary>
	///     An implementation of the dataAccess pattern to access database tables in a read-only way
	///     Dependencies:
	///     <see cref="ICircuitManager" />
	///     Settings:
	///     [None]
	/// </summary>
	/// <typeparam name="TDatabaseContext">The type of database to connect to</typeparam>
	/// <typeparam name="TEntity">The table (entity) to query</typeparam>
	public class ShardedDataAccess<TDatabaseContext, TEntity> : IShardedReadDataAccess<TEntity>, IAsyncDisposable
		where TDatabaseContext : ShardedDatabaseContext
		where TEntity : class, IAuditableEntity, ISoftDeleteEntity, IEntity, new()
	{
		protected IServiceProvider ServiceProvider { get; }

		public ShardedDataAccess(IServiceProvider serviceProvider, Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask, AsyncRetryPolicy? retryPolicy = null)
		{
			ServiceProvider = serviceProvider;
			ReusableDatabaseContext = new ReusableShardedDbContext<TDatabaseContext>(createShardedDatabaseContextFunctionWithTask);
			AsyncRetryPolicy = retryPolicy ?? RetryPolicyBuilder.Build(serviceProvider).WithWaitAndRetry();
		}

		public ShardedDataAccess(IServiceProvider serviceProvider, TDatabaseContext usedShardedDatabaseContext, Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask, AsyncRetryPolicy? retryPolicy = null)
		{
			ServiceProvider = serviceProvider;
			ReusableDatabaseContext = new ReusableShardedDbContext<TDatabaseContext>(usedShardedDatabaseContext, createShardedDatabaseContextFunctionWithTask);
			AsyncRetryPolicy = retryPolicy ?? RetryPolicyBuilder.Build(serviceProvider).WithWaitAndRetry();
		}

		public ReusableShardedDbContext<TDatabaseContext> ReusableDatabaseContext { get; }

		/// <summary>
		///     The retry policy to apply for retry operations
		/// </summary>
		private AsyncRetryPolicy? AsyncRetryPolicy { get; }

		/// <summary>
		///     The circuit breaker which will be used for calling the database
		/// </summary>
		private ICircuitBreaker CircuitBreaker => ServiceProvider.GetRequiredService<ICircuitManager>().GetCircuitBreaker(DependencyType.Storage, ExternalDependency.PlatformDatabase.ToString());

		public async Task<TResult> CommunicateWithDatabaseAsync<TResult>(Func<TDatabaseContext, Task<TResult>> communicationPredicate)
		{
			var databaseContext = await ReusableDatabaseContext.GetOrCreateContextAsync();
			return await CircuitBreaker.CommunicateAsync(() => communicationPredicate.Invoke(databaseContext), AsyncRetryPolicy);
		}

		public async ValueTask DisposeAsync()
		{
			if (!ReusableDatabaseContext.IsDisposed)
			{
				await ReusableDatabaseContext.DisposeAsync();
			}
		}

		public virtual async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, bool isDeleted = false, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeEntities = null)
		{
			// ReSharper disable once ArrangeRedundantParentheses - required for lambda to work
			includeEntities ??= (e => e);
			var entities = await CommunicateWithDatabaseAsync(async databaseContext => await includeEntities(databaseContext.Set<TEntity>().Where(filter).Where(entity => entity.IsDeleted == isDeleted)).ToListAsync());

			return entities;
		}

		public virtual async Task<TEntity> GetByExternalIdAsync(string id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeEntities = null)
		{
			// ReSharper disable once ArrangeRedundantParentheses - required for lambda to work
			includeEntities ??= (e => e);
			var entity = await CommunicateWithDatabaseAsync(async databaseContext =>
			{
				var matchingEntity = await includeEntities(databaseContext.Set<TEntity>()).SingleOrDefaultAsync(dbEntity => dbEntity.ExternalId == id);
				return matchingEntity;
			});

			return entity;
		}

		public async Task<ShardedDatabaseContext> GetOrCreateShardedDatabaseContextAsync()
		{
			return await ReusableDatabaseContext.GetOrCreateContextAsync();
		}

		/// <summary>
		///     Gets the entities from the managed cache
		/// </summary>
		/// <param name="cacheKey">Unique key per query - ensure that all includes are part of the key!</param>
		/// <param name="cacheRegion">The conceptual region which can be cleared</param>
		/// <param name="query">The query to execute</param>
		/// <typeparam name="T">The entity type to return</typeparam>
		/// <remarks>
		///     Include statements are cached by entity framework so the hash code can be used, but only clear using the cache
		///     region and not the cache key
		/// </remarks>
		public async Task<T?> GetCachedItemsAsync<T>(string cacheKey, string cacheRegion, Func<Task<T>> query)
			where T : class, new()
		{
			var databaseContext = await ReusableDatabaseContext.GetOrCreateContextAsync();

			var cache = await ServiceProvider.GetInvalidationEnabledCacheAsync();

			if (cache != null)
			{
				var existingCachedItems = await cache.GetAsync<T>(cacheKey, cacheRegion);
				if (existingCachedItems.IsPresent)
				{
					var customProperties = new Dictionary<string, string>
					{
						{CustomProperties.CorrelationId, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId()}
					};

					ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Database Query Cache Hit", value: 1.0, customProperties);
					return existingCachedItems.Value;
				}
			}

			var autoDetectChangesEnabledDefault = databaseContext.ChangeTracker.AutoDetectChangesEnabled;

			try
			{
				databaseContext.ChangeTracker.AutoDetectChangesEnabled = false;

				var item = await query();

				if (cache == null)
				{
					return item;
				}

				if (item is ICollection collection)
				{
					if (collection.Count > 0)
					{
						await cache.AddAsync(cacheKey, item, cacheRegion, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId());
					}
				}
				else
				{
					await cache.AddAsync(cacheKey, item, cacheRegion, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId());
				}

				return item;
			}
			finally
			{
				databaseContext.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabledDefault;
			}
		}
	}
}