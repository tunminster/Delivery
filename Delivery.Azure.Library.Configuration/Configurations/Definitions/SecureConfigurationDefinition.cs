using System;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Configuration.Configurations.Definitions
{
    public class SecureConfigurationDefinition : ConfigurationDefinition
    {
        protected SecureConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected ISecretProvider SecretProvider => ServiceProvider.GetRequiredService<ISecretProvider>();
    }
}