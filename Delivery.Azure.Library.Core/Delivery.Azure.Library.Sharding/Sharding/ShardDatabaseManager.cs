using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Sharding.Sharding
{
    public class ShardDatabaseManager : IShardDatabaseManager
    {
        private readonly IServiceProvider serviceProvider;

        public ShardDatabaseManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<string> GetAdministratorConnectionStringAsync(IShard shard)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetConnectionStringAsync(IShard shard)
        {
            throw new NotImplementedException();
        }
    }
}
