using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.CronJobs.Host.Services
{
    public class DriverTimerRejectionService
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverTimerRejectionService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task RunAsync(IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverTimerRejectionService)} has started.");
            await new DriverTimerRejectionCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new DriverTimerRejectionCommand(executingRequestContextAdapter.GetShard().Key));
            
        }
    }
}