using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Exceptions;
using Delivery.Azure.Library.Sharding.Exceptions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Sharding.Sharding
{
    /// <summary>
    ///     Manages connections to database shards in the platform
    ///     Dependencies:
    ///     <see cref="IConfigurationProvider" />
    ///     <see cref="ISecretProvider" />
    ///     Settings:
    ///     [None]
    /// </summary>
    public class ShardDatabaseManager : IShardDatabaseManager
    {
        private readonly IServiceProvider serviceProvider;

        public ShardDatabaseManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///     Gets the connection string to a shard using <see cref="ISecretProvider" />
        /// </summary>
        /// <param name="shard">Information about the shard</param>
        /// <exception cref="ShardNotFoundException">Exception thrown when the specified shard was not found</exception>
        public async Task<string> GetConnectionStringAsync(IShard shard)
        {
            var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

            try
            {
                var connectionString = await secretProvider.GetSecretAsync($"Sql-Database-{shard.Key}-Connection-String");

                return connectionString;
            }
            catch (SecretNotFoundException exception)
            {
                throw new ShardNotFoundException(shard, exception);
            }
        }

        /// <summary>
        ///     Gets the connection string to a server admin user with rights to modify database schemas <see cref="ISecretProvider" />
        /// </summary>
        /// <param name="shard">Information about the shard</param>
        /// <exception cref="ShardNotFoundException">Exception thrown when the specified shard was not found</exception>
        public async Task<string> GetAdministratorConnectionStringAsync(IShard shard)
        {
            var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();

            try
            {
                var connectionString = await secretProvider.GetSecretAsync($"Sql-Database-{shard.Key}-Administrator-Connection-String");
                return connectionString;
            }
            catch (SecretNotFoundException exception)
            {
                throw new ShardNotFoundException(shard, exception);
            }
        }
    }
}