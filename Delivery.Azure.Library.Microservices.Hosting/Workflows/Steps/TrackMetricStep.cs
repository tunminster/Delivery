using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Steps
{
    public class TrackMetricStep : StepContextBodyAsync<TrackMetricStepCommand>
    {
        public TrackMetricStep(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async Task RunCoreAsync(IStepExecutionContext context)
        {
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric(Input.MetricName, Input.MetricValue, Input.ExecutingRequestContext.GetTelemetryProperties());
            await Task.CompletedTask;
        }

        protected override ExecutingRequestContext ExecutingRequestContext => Input.ExecutingRequestContext;
    }
}