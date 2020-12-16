using System;
using Delivery.Azure.Library.Configuration.Configurations;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Configuration.Environments.Configurations
{
    public class EnvironmentConfigurationDefinition : ConfigurationDefinition
    {
        protected const string EnvironmentSettingKey = "ASPNETCORE_ENVIRONMENT";

        protected const string RingSettingKey = "Ring";

        /// <summary>
        ///     The setting for the current environment which the application is running in
        ///     If used in combination with the <see cref="EnvironmentProvider" /> this should match the
        ///     <see cref="RuntimeEnvironment" /> enum
        /// </summary>
        public virtual string Environment => ConfigurationProvider.GetSetting<string>(EnvironmentSettingKey);

        /// <summary>
        ///     Gets the ring from the configuration provider
        /// </summary>
        public virtual int? Ring => ConfigurationProvider.GetSettingOrDefault<int?>(RingSettingKey, defaultValue: null);

        public EnvironmentConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}