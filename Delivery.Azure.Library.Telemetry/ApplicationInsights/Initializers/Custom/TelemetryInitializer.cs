using System;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers.Custom
{
    public abstract class TelemetryInitializer : ITelemetryInitializer
    {
        protected IServiceProvider ServiceProvider { get; }

        protected TelemetryInitializer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void Initialize(ITelemetry telemetry)
        {
            OnInitialize(telemetry);

            if (string.IsNullOrEmpty(telemetry.Context.InstrumentationKey))
            {
                var instrumentationKey = ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("ApplicationInsights_InstrumentationKey", isMandatory: false);
                if (string.IsNullOrEmpty(instrumentationKey))
                {
                    instrumentationKey = ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("ApplicationInsights-InstrumentationKey");
                }

                telemetry.Context.InstrumentationKey = instrumentationKey;
            }
        }

        protected abstract void OnInitialize(ITelemetry telemetry);
    }
}