using System;
using System.Reflection;
using Delivery.Azure.Library.Core.Extensions.Assemblies;
using Microsoft.ApplicationInsights.Channel;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers.Custom
{
    public class VersionTelemetryInitializer : TelemetryInitializer
    {
        private readonly Assembly sourceAssembly;

        public VersionTelemetryInitializer(IServiceProvider serviceProvider, Assembly sourceAssembly) : base(serviceProvider)
        {
            this.sourceAssembly = sourceAssembly;
        }

        protected override void OnInitialize(ITelemetry telemetry)
        {
            var environmentVersion = Environment.GetEnvironmentVariable("Version");
            if (string.IsNullOrEmpty(environmentVersion))
            {
                environmentVersion = sourceAssembly.GetAssemblyVersion();
            }

            telemetry.Context.Component.Version = environmentVersion;
        }
    }
}