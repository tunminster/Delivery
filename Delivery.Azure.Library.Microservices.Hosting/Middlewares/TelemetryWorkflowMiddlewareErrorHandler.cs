using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Delivery.Azure.Library.Microservices.Hosting.Middlewares
{
    public class TelemetryWorkflowMiddlewareErrorHandler : IWorkflowMiddlewareErrorHandler
    {
        private readonly IServiceProvider serviceProvider;

        public TelemetryWorkflowMiddlewareErrorHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public Task HandleAsync(Exception ex)
        {
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(ex);
            return Task.CompletedTask;
        }
    }
}