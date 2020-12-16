using System;
using Delivery.Azure.Library.Authentication.ActiveDirectory.Configurations;
using Delivery.Azure.Library.Configuration.Configurations;

namespace Delivery.Azure.Library.KeyVault.Providers.Configurations
{
    public class KeyVaultSecretConfigurationDefinition : ActiveDirectoryAuthenticationConfigurationDefinition
    {
        /// <summary>
        ///     Uri to the Azure Key Vault that is being consulted
        /// </summary>
        public virtual string VaultUri => ConfigurationProvider.GetSetting<string>("KeyVault_Uri");

        /// <summary>
        ///     If the Azure Managed Service Identity (MSI) environment variables are set assume by default that MSI is being used
        ///     to connect to key vault
        /// </summary>
        public virtual bool UseManagedServiceIdentity => !string.IsNullOrEmpty(ConfigurationProvider.GetSetting("MSI_ENDPOINT", isMandatory: false));

        public KeyVaultSecretConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}