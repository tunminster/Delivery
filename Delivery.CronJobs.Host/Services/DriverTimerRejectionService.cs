using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection;

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
            await new DriverTimerRejectionCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new DriverTimerRejectionCommand(executingRequestContextAdapter.GetShard().Key));
            
        }
    }
}