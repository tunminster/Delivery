using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Azure.Library.Database.DataAccess.Interfaces
{
    /// <summary>
    ///     Exposes write methods to access the database entities
    /// </summary>
    public interface IShardedWriteDataAccess<TEntity>
        where TEntity : class, ISoftDeleteEntity, IEntity, new()
    {
        /// <summary>
        ///     Marks a <typeparamref name="TEntity" /> as inactive (soft delete)
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <param name="executingRequestContextAdapter">The name of the authenticated user who deleted the entry</param>
        Task<TEntity> DeleteAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, TEntity entity);

        /// <summary>
        ///     Inserts a new <typeparamref name="TEntity" /> into the table
        /// </summary>
        /// <param name="entity">The entity to insert</param>
        /// <param name="executingRequestContextAdapter">The name of the authenticated user who added the entity</param>
        Task<TEntity> AddAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, TEntity entity);
    }
}