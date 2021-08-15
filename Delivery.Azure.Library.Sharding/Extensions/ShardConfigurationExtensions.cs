using System;
using System.Linq;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Sharding.Extensions
{
    public static class ShardConfigurationExtensions
    {
        /// <summary>
        ///     Shard key configurations can be set as comma-separated list of shard keys
        /// </summary>
        /// <param name="shard">The shard to check is enabled</param>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="configurationSettingName">The configuration setting key</param>
        /// <returns><c>True</c> if the setting is on</returns>
        public static bool IsShardConfigurationEnabled(this IShard shard, IServiceProvider serviceProvider, string configurationSettingName)
        {
            var shardKey = shard.Key.Replace(" ", "");
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            var configurationSetting = configurationProvider.GetSetting(configurationSettingName, isMandatory: false);
            if (string.IsNullOrEmpty(configurationSetting))
            {
                return false;
            }

            var tenantList = configurationSetting.Split(separator: ',', StringSplitOptions.RemoveEmptyEntries).Select(tenantName => tenantName.Replace(" ", "").ToLowerInvariant());
            var isShardConfigurationEnabled = tenantList.Contains(shardKey.ToLowerInvariant());
            return isShardConfigurationEnabled;
        }
    }
}