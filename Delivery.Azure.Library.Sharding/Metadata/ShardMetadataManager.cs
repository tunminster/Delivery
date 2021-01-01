using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Sharding.Metadata
{
    public class ShardMetadataManager : IShardMetadataManager
    {
        private readonly IServiceProvider serviceProvider;

        public ShardMetadataManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public List<TShardInformation> GetShardInformation<TShardInformation>()
            where TShardInformation : ShardInformation
        {
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            var shardValue = configurationProvider.GetSetting("Shards");
            var shardInformation = shardValue.ConvertFromJson<List<TShardInformation>>();
            return shardInformation;
        }

        public TShardInformation GetShardInformation<TShardInformation>(IShard shard)
            where TShardInformation : ShardInformation
        {
            var shardInformation = GetShardInformation<TShardInformation>();
            if (shardInformation.Any(info => string.IsNullOrEmpty(info.Key)))
            {
                throw new InvalidOperationException($"All shards should have a {nameof(ShardInformation.Key)} property set. Found: {shardInformation.Format()}");
            }

            var information = shardInformation.Single(info => Shard.Create(info.DisplayName!).Equals(shard));
            return information;
        }

        public List<IShard> GetShards(bool isActive = true)
        {
            var shardInformation = GetShardInformation<ShardInformation>();
            if (shardInformation.Any(info => string.IsNullOrEmpty(info.Key)))
            {
                throw new InvalidOperationException($"All shards should have a {nameof(ShardInformation.Key)} property set. Found: {shardInformation.Format()}");
            }

            var shards = shardInformation.Where(info => info.IsActive == isActive).Select(info => Shard.Create(info.Key!)).ToList();
            return shards;
        }
    }
}