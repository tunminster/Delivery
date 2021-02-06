using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreUpdate;

namespace Delivery.Store.Domain.Handlers.MessageHandlers.StoreUpdate
{
    public class StoreUpdateMessagePublisher : IMessagePublisherAsync<StoreUpdateMessage>
    {
        private readonly IServiceProvider serviceProvider;

        public StoreUpdateMessagePublisher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task PublishAsync(StoreUpdateMessage message)
        {
            var executingContextAdapter = new ExecutingRequestContextAdapter(message.RequestContext);
            
            var cloudEventMessage = message
                .CreateCloudEventMessage(serviceProvider, executingContextAdapter)
                .WithExecutingContext(executingContextAdapter);
            
            await serviceProvider.GetRequiredHostedService<IQueueServiceBusWorkBackgroundService>()
                .EnqueueBackgroundWorkAsync(OrderConstants.ServiceBusEntityName,
                    OrderConstants.ServiceBusConnectionStringName, cloudEventMessage);
        }
    }
}