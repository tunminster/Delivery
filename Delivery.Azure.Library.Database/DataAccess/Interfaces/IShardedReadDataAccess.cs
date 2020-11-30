using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Database.Context;

namespace Delivery.Azure.Library.Database.DataAccess.Interfaces
{
    /// <summary>
    ///     Exposes read-only methods to access the database entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IShardedReadDataAccess<TEntity>
        where TEntity : class, IAuditableEntity, ISoftDeleteEntity, IEntity, new()
    {
        /// <summary>
        ///     Allows callers to access the used database context
        /// </summary>
        /// <returns>The used database context</returns>
        Task<ShardedDatabaseContext> GetOrCreateShardedDatabaseContextAsync();

        /// <summary>
        ///     Gets a <typeparamref name="TEntity" /> by its identifier
        /// </summary>
        /// <param name="id">The entity id</param>
        /// <param name="includeEntities">Allows the query to select which related entities to include to the results</param>
        Task<TEntity> GetByExternalIdAsync(string id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeEntities = null);

        /// <summary>
        ///     Gets a list of <typeparamref name="TEntity" /> using insertion timestamp logic and applies a filter over the set
        ///     afterwards
        /// </summary>
        /// <param name="filter">Reduces the entities to a filtered query</param>
        /// <param name="isDeleted">If <c>True</c> then only active results are returned, <c>False</c> includes deleted entities</param>
        /// <param name="includeEntities">Allows the query to select which related entities to include to the results</param>
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, bool isDeleted = false, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeEntities = null);
    }
}