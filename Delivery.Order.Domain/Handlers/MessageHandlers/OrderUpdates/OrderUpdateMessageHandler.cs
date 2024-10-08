using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Enums;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Contracts.V1.MessageContracts.PushNotification;
using Delivery.Order.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate;
using Delivery.Order.Domain.Enum;
using Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate;
using Delivery.Order.Domain.Handlers.MessageHandlers.OrderIndexing;
using Delivery.Order.Domain.Handlers.MessageHandlers.PushNotification;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Handlers.MessageHandlers.OrderUpdates
{
    public class OrderUpdateMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public OrderUpdateMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(OrderUpdateMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StripeOrderUpdateContract, StripeOrderUpdateStatusContract>(message);
                var orderId = string.Empty;
                var orderPaymentStatus = OrderPaymentStatus.None;
                if (!processingStates.HasFlag(MessageProcessingStates.PersistOrder))
                {
                    var orderUpdateCommand =
                        new OrderUpdateCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());
                    
                    var orderUpdateCommandHandler =
                        new OrderUpdateCommandHandler(ServiceProvider, ExecutingRequestContextAdapter);
                    var stripeOrderUpdateStatusContract = await orderUpdateCommandHandler.HandleAsync(orderUpdateCommand);
                    orderId = stripeOrderUpdateStatusContract.OrderId;
                    orderPaymentStatus = stripeOrderUpdateStatusContract.PaymentStatusEnum;
                    processingStates |= MessageProcessingStates.PersistOrder;
                }
                
                
                if (!processingStates.HasFlag(MessageProcessingStates.Processed) && !string.IsNullOrEmpty(orderId) && orderPaymentStatus == OrderPaymentStatus.Succeed)
                {
                    var orderCreatedPushNotificationMessageContract = new OrderCreatedPushNotificationMessageContract
                    {
                        PayloadIn = new OrderCreatedPushNotificationRequestContract { OrderId = orderId },
                        PayloadOut = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow },
                        RequestContext = message.RequestContext
                    };

                    await new OrderCreatedPushNotificationMessagePublisher(ServiceProvider).PublishAsync(orderCreatedPushNotificationMessageContract);
                    
                    // Indexing order
                    await new OrderIndexMessagePublisher(ServiceProvider).PublishAsync(
                        new OrderIndexMessageContract
                        {
                            PayloadIn = new IndexCreationContract { Id = orderId },
                            PayloadOut = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow },
                            RequestContext = message.RequestContext
                        });
                    
                    ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Sent {nameof(OrderCreatedPushNotificationMessagePublisher)} - {orderCreatedPushNotificationMessageContract.ConvertToJson()}", SeverityLevel.Information, ExecutingRequestContextAdapter.GetTelemetryProperties());
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
                else
                {
                    ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace(
                        $"{nameof(OrderCreatedPushNotificationMessagePublisher)} didn't send for orderId:{orderId} and payment status:{orderPaymentStatus}",
                        SeverityLevel.Critical, ExecutingRequestContextAdapter.GetTelemetryProperties());
                }
                
                // complete
                processingStates |= MessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Order update persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}