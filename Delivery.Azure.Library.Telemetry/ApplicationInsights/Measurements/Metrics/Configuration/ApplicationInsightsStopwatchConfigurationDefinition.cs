using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics.Configuration
{
    public class ApplicationInsightsStopwatchConfigurationDefinition : ConfigurationDefinition
    {
        private const string StopwatchEnabledSettingName = "Stopwatch_Enabled";

        /// <summary>
        ///     Toggle to turn the stopwatch on or off
        /// </summary>
        public virtual bool IsEnabled => ConfigurationProvider.GetSettingOrDefault<bool>(StopwatchEnabledSettingName, defaultValue: true);

        public ApplicationInsightsStopwatchConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}