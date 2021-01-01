using System.Collections.Generic;
using Delivery.Azure.Library.Sharding.Contracts.V1;

namespace Delivery.Azure.Library.Sharding.Interfaces
{
    public interface IShardMetadataManager
    {
        /// <summary>
        ///     Gets extended details about the shards
        /// </summary>
        /// <typeparam name="TShardInformation">The type of shard information</typeparam>
        List<TShardInformation> GetShardInformation<TShardInformation>() where TShardInformation : ShardInformation;

        /// <summary>
        ///     Gets the basic list of shards
        /// </summary>
        /// <param name="isActive">Filter by active shards</param>
        List<IShard> GetShards(bool isActive = true);
    }
}