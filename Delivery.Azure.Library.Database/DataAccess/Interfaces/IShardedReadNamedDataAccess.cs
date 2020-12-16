using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Core.Monads;

namespace Delivery.Azure.Library.Database.DataAccess.Interfaces
{
    /// <summary>
    ///     Encapsulates common read queries for a table with a name-column
    /// </summary>
    /// <typeparam name="TEntity">The entity/table type</typeparam>
    public interface IShardedReadNamedDataAccess<TEntity> : IShardedReadDataAccess<TEntity>
        where TEntity : class, IAuditableEntity, ISoftDeleteEntity, IEntity, new()
    {
        /// <summary>
        ///     Gets a <typeparamref name="TEntity" /> by its name
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <param name="includeEntities">Allows the query to select which related entities to include to the results</param>
        Task<Maybe<TEntity>> GetByNameAsync(string name, Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeEntities = null);
    }
}