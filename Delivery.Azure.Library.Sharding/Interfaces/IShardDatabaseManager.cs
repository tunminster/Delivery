using System.Threading.Tasks;

namespace Delivery.Azure.Library.Sharding.Interfaces
{
    /// <summary>
    ///     Manages connections to database shards in the platform
    /// </summary>
    public interface IShardDatabaseManager
    {
        /// <summary>
        ///     Gets the runtime connection string to a shard
        /// </summary>
        /// <param name="shard">Information about the shard</param>
        /// <exception cref="ShardNotFoundException">Exception thrown when the specified shard was not found</exception>
        Task<string> GetConnectionStringAsync(IShard shard);

        /// <summary>
        ///     Gets the administrator connection string to a shard with privileges to modify the database schema
        /// </summary>
        /// <param name="shard">Information about the shard</param>
        /// <exception cref="ShardNotFoundException">Exception thrown when the specified shard was not found</exception>
        Task<string> GetAdministratorConnectionStringAsync(IShard shard);
    }
}