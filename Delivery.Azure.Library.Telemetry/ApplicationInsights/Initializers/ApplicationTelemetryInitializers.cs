using System;
using System.Collections.Generic;
using System.Reflection;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers.Custom;
using Microsoft.ApplicationInsights.Extensibility;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers
{
    public class ApplicationTelemetryInitializers : List<ITelemetryInitializer>
    {
        public ApplicationTelemetryInitializers(IServiceProvider serviceProvider, Assembly sourceAssembly)
        {
            // Custom telemetry initializers
            Add(new VersionTelemetryInitializer(serviceProvider, sourceAssembly));
            Add(new ApplicationMapTelemetryInitializer(serviceProvider));
        }
    }
}