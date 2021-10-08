using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Enums;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.CommandHandlers.PushNotification;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement
{
    public class ShopOrderCompleteMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopOrderCompleteMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(ShopOrderCompleteMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<EntityUpdateContract, StatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    // remove shop order indexing
                    var shopOrderRemoveIndexCommand = new ShopOrderRemoveIndexCommand(messageAdapter.GetPayloadIn().Id);

                    await new ShopOrderRemoveIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(shopOrderRemoveIndexCommand);
                    
                    
                    // update order
                    var shopOrderStatusCreationContract = new ShopOrderStatusCreationContract
                    {
                        OrderId = shopOrderRemoveIndexCommand.OrderId,
                        OrderStatus = OrderStatus.Completed,
                        PreparationTime = 10
                    };

                    var shopOrderStatusCommand = new ShopOrderStatusCommand(shopOrderStatusCreationContract);
                    await new ShopOrderStatusCommandHandler(ServiceProvider, ExecutingRequestContextAdapter).Handle(
                        shopOrderStatusCommand);
                    
                    // push notification to user and shop owner
                    var orderCompletePushNotificationCommand = new OrderCompletePushNotificationCommand(
                        new ShopOrderCompletePushNotificationCreationContract
                            { OrderId = messageAdapter.GetPayloadIn().Id });
                    await new OrderCompletePushNotificationCommandHandler(ServiceProvider,
                            ExecutingRequestContextAdapter)
                        .Handle(orderCompletePushNotificationCommand);
                        
                    processingStates |= OrderMessageProcessingStates.Processed;
                }
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Shop order delivered",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}