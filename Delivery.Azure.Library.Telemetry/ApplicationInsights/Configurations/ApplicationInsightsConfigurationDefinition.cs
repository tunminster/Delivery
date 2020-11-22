using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Configurations
{
    public class ApplicationInsightsConfigurationDefinition : ConfigurationDefinition
	{
		private readonly string instrumentationKeySettingName;

		protected override IConfigurationProvider ConfigurationProvider { get; }

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="source">Source of the telemetry</param>
		public ApplicationInsightsConfigurationDefinition(IServiceProvider serviceProvider, string source) : this(serviceProvider, source, "ApplicationInsights-Platform-InstrumentationKey")
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="source">Source of the telemetry</param>
		/// <param name="instrumentationKeySettingName">Name of the setting that contains the instrumentation key</param>
		public ApplicationInsightsConfigurationDefinition(IServiceProvider serviceProvider, string source, string instrumentationKeySettingName) : base(serviceProvider)
		{
			Source = source;
			this.instrumentationKeySettingName = instrumentationKeySettingName;
			ConfigurationProvider = ServiceProvider.GetRequiredService<IConfigurationProvider>();
		}

		/// <summary>
		///     The key which is used to authenticate this application in application insights and allows the telemetry messages to
		///     be sent
		/// </summary>
		public virtual string InstrumentationKey => ConfigurationProvider.GetSetting<string>(instrumentationKeySettingName);

		/// <summary>
		///     Indicates if only the source (e.g. Framework) is disabled. Other sources (e.g. Application) are not affected
		/// </summary>
		public virtual bool IsSourceDisabled => ConfigurationProvider.GetSettingOrDefault<bool>($"ApplicationInsights_SourceDisabled_{Source}", defaultValue: false);

		/// <summary>
		///     Initial percentage to use for sampling telemetry.
		///     For more information, see
		///     https://docs.microsoft.com/en-us/azure/application-insights/app-insights-sampling#configuring-adaptive-sampling
		/// </summary>
		public virtual double InitialSamplingPercentage => ConfigurationProvider.GetSettingOrDefault<double>("ApplicationInsights_Sampling_InitialPercentage", defaultValue: 100);

		/// <summary>
		///     Adds the source of the telemetry event to the custom properties
		/// </summary>
		/// <example>Application, Framework etc</example>
		public virtual string Source { get; }
	}
}