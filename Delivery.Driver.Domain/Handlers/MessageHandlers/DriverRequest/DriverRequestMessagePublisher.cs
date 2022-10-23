using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Driver.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverRequest
{
    public class DriverRequestMessagePublisher : MessagePublisherAsync<DriverRequestMessageContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverRequestMessagePublisher(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        protected override async Task PublishAsync(DriverRequestMessageContract message)
        {
            var executingContextAdapter = new ExecutingRequestContextAdapter(message.RequestContext);
            var cloudEventMessage = message
                .CreateCloudEventMessage(serviceProvider, executingContextAdapter)
                .WithExecutingContext(executingContextAdapter);
            
            await serviceProvider.GetRequiredHostedService<IQueueServiceBusWorkBackgroundService>()
                .EnqueueBackgroundWorkAsync(Delivery.Domain.Constants.Constants.DriverServiceBusEntityName,
                    Delivery.Domain.Constants.Constants.DriverServiceBusConnectionStringName, cloudEventMessage);
        }
    }
}