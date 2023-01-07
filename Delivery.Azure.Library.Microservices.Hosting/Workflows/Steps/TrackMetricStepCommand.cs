using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Steps
{
    public record TrackMetricStepCommand(ExecutingRequestContext ExecutingRequestContext, string MetricName,
        double MetricValue) : IWorkflowExecutingRequest;

}