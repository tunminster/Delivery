using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Database.DataAccess.Interfaces
{
    /// <summary>
    ///     Provides structured and managed access to database entities
    /// </summary>
    // ReSharper disable once UnusedTypeParameter
    public interface IShardedDataAccess<TEntity> : IShardAware
        where TEntity : class, IAuditableEntity, ISoftDeleteEntity, IEntity, new()
    {
    }
}