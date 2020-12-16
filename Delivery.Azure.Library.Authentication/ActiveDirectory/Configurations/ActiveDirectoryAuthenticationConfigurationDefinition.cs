using System;
using Delivery.Azure.Library.Configuration.Configurations;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Authentication.ActiveDirectory.Configurations
{
    public class ActiveDirectoryAuthenticationConfigurationDefinition : SecureConfigurationDefinition
    {
        /// <summary>
        ///     The url which provides a graph api for querying
        /// </summary>
        public virtual string GraphUrl { get; } = "https://graph.microsoft.com";

        public string GetClientId()
        {
            return ConfigurationProvider.GetSetting<string>("KeyVault_ActiveDirectory_ClientId");
        }

        public string GetAppKey()
        {
            return ConfigurationProvider.GetSetting<string>("KeyVault_ActiveDirectory_AppKey");
        }

        private string DefaultTenantId => ConfigurationProvider.GetSetting("ActiveDirectory_Tenant_Id");

        /// <summary>
        ///     The active directory identity provider which will handle logins for a shard
        /// </summary>
        public string GetAuthority(string? tenantId = null)
        {
            return $"https://login.windows.net/{tenantId ?? DefaultTenantId}";
        }

        public ActiveDirectoryAuthenticationConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}