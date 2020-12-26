using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Storage.Cosmos.Configurations
{
    public class CosmosDatabaseConnectionConfigurationDefinition : ConfigurationDefinition
    {
        public CosmosDatabaseConnectionConfigurationDefinition(IServiceProvider serviceProvider, IShard shard) : base(serviceProvider)
        {
            SecretName = $"CosmosDatabase-{shard.Key}-ConnectionString";
        }
        
        public CosmosDatabaseConnectionConfigurationDefinition(IServiceProvider serviceProvider, string secretName = "CosmosDatabase-ConnectionString") : base(serviceProvider)
        {
            SecretName = secretName;
        }
        
        public virtual string SecretName { get; }
    }
}