using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Order.Domain.Contracts.V1.MessageContracts.PushNotification;
using Delivery.Order.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Order.Domain.Handlers.CommandHandlers.PushNotification;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Handlers.MessageHandlers.PushNotification
{
    public class OrderCreatedPushNotificationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public OrderCreatedPushNotificationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(OrderCreatedPushNotificationMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<OrderCreatedPushNotificationRequestContract, StatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var orderCreatedPushNotificationCommand =
                        new OrderCreatedPushNotificationCommand(messageAdapter.GetPayloadIn());

                    await new OrderCreatedPushNotificationCommandHandler(ServiceProvider,
                        ExecutingRequestContextAdapter).Handle(orderCreatedPushNotificationCommand);
                    
                    ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                        .TrackTrace($"{nameof(OrderCreatedPushNotificationCommandHandler)} processed {orderCreatedPushNotificationCommand.OrderCreatedPushNotificationRequestContract.ConvertToJson()}", SeverityLevel.Information, ExecutingRequestContextAdapter.GetTelemetryProperties());
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("A new order push notification sent",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}