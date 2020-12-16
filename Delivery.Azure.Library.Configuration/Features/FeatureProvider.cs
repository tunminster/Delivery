using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Delivery.Azure.Library.Configuration.Features
{
    /// <summary>
    ///     Allows specific features to be enabled or disabled by configuration
    /// </summary>
    public class FeatureProvider : IFeatureProvider
    {
        private readonly IServiceProvider serviceProvider;
        private ConcurrentBag<string>? cachedFeatureNames;

        public FeatureProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<bool> IsEnabledAsync(string featureFlagKey, bool isEnabledByDefault = true)
        {
            var configuredFeatureFlagName = $"FeatureFlag_{featureFlagKey}";
            var configFileFeature = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<bool?>(configuredFeatureFlagName, defaultValue: null);
            if (configFileFeature.HasValue)
            {
                return configFileFeature.Value;
            }

            var appConfigurationFeatureFlag = serviceProvider.GetService<IFeatureManager>();
            if (appConfigurationFeatureFlag == null)
            {
                return isEnabledByDefault;
            }

            if (cachedFeatureNames == null)
            {
                cachedFeatureNames = new ConcurrentBag<string>();
                var features = appConfigurationFeatureFlag.GetFeatureNamesAsync();
                await foreach (var featureName in features)
                {
                    cachedFeatureNames.Add(featureName);
                }
            }

            var isFeatureFlagExist = false;
            foreach (var featureName in cachedFeatureNames)
            {
                if (featureName.Equals(featureFlagKey))
                {
                    isFeatureFlagExist = true;
                }
            }

            if (!isFeatureFlagExist)
            {
                return isEnabledByDefault;
            }

            var isEnabled = await appConfigurationFeatureFlag.IsEnabledAsync(featureFlagKey);
            return isEnabled;
        }
    }
}