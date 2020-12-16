using Delivery.Azure.Library.Core.Guards;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Processors
{
    public class SampleSuccessfulDependenciesProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;
        private readonly AdaptiveSamplingTelemetryProcessor samplingProcessor;

        public SampleSuccessfulDependenciesProcessor(ITelemetryProcessor next, double initialSamplingPercentage)
        {
            Guard.Against(initialSamplingPercentage < 0, nameof(initialSamplingPercentage));

            this.next = next;
            samplingProcessor = CreateSamplingProcessor(initialSamplingPercentage);
        }

        private AdaptiveSamplingTelemetryProcessor CreateSamplingProcessor(double initialSamplingPercentage)
        {
            var adaptiveSamplingProcessor = new AdaptiveSamplingTelemetryProcessor(next)
            {
                InitialSamplingPercentage = initialSamplingPercentage
            };

            return adaptiveSamplingProcessor;
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry dependencyTelemetryItem && dependencyTelemetryItem.Success == true)
            {
                samplingProcessor.Process(dependencyTelemetryItem);
                return;
            }

            next.Process(item);
        }
    }
}