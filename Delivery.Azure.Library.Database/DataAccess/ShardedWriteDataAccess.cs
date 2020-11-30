using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Database.DataAccess.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Polly.Retry;

namespace Delivery.Azure.Library.Database.DataAccess
{
    /// <summary>
    ///     An implementation of the dataAccess pattern to access database tables for inserts and updated
    ///     Dependencies:
    ///     <see cref="ICircuitManager" />
    ///     Settings:
    ///     [None]
    /// </summary>
    /// <typeparam name="TDatabaseContext">The type of database to connect to</typeparam>
    /// <typeparam name="TEntity">The table (entity) to query</typeparam>
    public sealed class ShardedWriteDataAccess<TDatabaseContext, TEntity> : ShardedDataAccess<TDatabaseContext, TEntity>, IShardedWriteDataAccess<TEntity> where TDatabaseContext : ShardedDatabaseContext
        where TEntity : class, ISoftDeleteEntity, IAuditableEntity, IEntity, new()
    {
        public ShardedWriteDataAccess(IServiceProvider serviceProvider, Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask, AsyncRetryPolicy? retryPolicy = null) : base(serviceProvider, createShardedDatabaseContextFunctionWithTask, retryPolicy)
        {
        }

        public ShardedWriteDataAccess(IServiceProvider serviceProvider, TDatabaseContext usedShardedDatabaseContext, Func<Task<TDatabaseContext>> createShardedDatabaseContextFunctionWithTask, AsyncRetryPolicy? retryPolicy = null)
            : base(serviceProvider, usedShardedDatabaseContext, createShardedDatabaseContextFunctionWithTask, retryPolicy)
        {
        }

        public async Task<TEntity> AddAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, TEntity entity)
        {
            await CommunicateWithDatabaseAsync(async databaseContext =>
            {
                await databaseContext.Set<TEntity>().AddAsync(entity);
                return await databaseContext.SaveChangesAsync();
            });

            return entity;
        }

        public async Task<TEntity> DeleteAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, TEntity entity)
        {
            await CommunicateWithDatabaseAsync(async databaseContext =>
            {
                entity.IsDeleted = true;

                return await databaseContext.SaveChangesAsync();
            });

            return entity;
        }
    }
}