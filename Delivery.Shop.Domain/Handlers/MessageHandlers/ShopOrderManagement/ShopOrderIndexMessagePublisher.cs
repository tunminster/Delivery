using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement
{
    public class ShopOrderIndexMessagePublisher : IMessagePublisherAsync<ShopOrderIndexMessageContract>
    {
        private readonly IServiceProvider serviceProvider;

        public ShopOrderIndexMessagePublisher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task PublishAsync(ShopOrderIndexMessageContract message)
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