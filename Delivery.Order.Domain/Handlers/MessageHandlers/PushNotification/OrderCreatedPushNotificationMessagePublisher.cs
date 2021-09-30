using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Order.Domain.Contracts.V1.MessageContracts.PushNotification;

namespace Delivery.Order.Domain.Handlers.MessageHandlers.PushNotification
{
    public class OrderCreatedPushNotificationMessagePublisher : IMessagePublisherAsync<OrderCreatedPushNotificationMessageContract>
    {
        private readonly IServiceProvider serviceProvider;

        public OrderCreatedPushNotificationMessagePublisher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task PublishAsync(OrderCreatedPushNotificationMessageContract message)
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