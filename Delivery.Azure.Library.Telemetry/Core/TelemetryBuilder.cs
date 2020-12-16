using System.Collections.Generic;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;

namespace Delivery.Azure.Library.Telemetry.Core
{
    public class TelemetryBuilder
    {
        private readonly List<IApplicationInsightsTelemetry> telemetrySinks;

        private TelemetryBuilder(IApplicationInsightsTelemetry telemetrySink)
        {
            telemetrySinks = new List<IApplicationInsightsTelemetry>
            {
                telemetrySink
            };
        }

        public static TelemetryBuilder AddInitialSink(IApplicationInsightsTelemetry telemetrySink)
        {
            return new TelemetryBuilder(telemetrySink);
        }

        public TelemetryBuilder AddSink(IApplicationInsightsTelemetry telemetrySink)
        {
            telemetrySinks.Add(telemetrySink);
            return this;
        }

        public HybridApplicationInsightsTelemetry Build()
        {
            return new HybridApplicationInsightsTelemetry(telemetrySinks);
        }
    }
}