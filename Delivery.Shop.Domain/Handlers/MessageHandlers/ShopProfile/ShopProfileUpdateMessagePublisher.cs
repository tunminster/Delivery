using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopProfile;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopProfile
{
    public class ShopProfileUpdateMessagePublisher : IMessagePublisherAsync<ShopProfileMessageContract>
    {
        private readonly IServiceProvider serviceProvider;

        public ShopProfileUpdateMessagePublisher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task PublishAsync(ShopProfileMessageContract message)
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