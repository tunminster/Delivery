using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate;
using Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Handlers.MessageHandlers.OrderStatusUpdates
{
    public class OrderStatusUpdateMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public OrderStatusUpdateMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(OrderStatusUpdateMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StripeUpdateOrderContract, StripeUpdateOrderStatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.PersistOrder))
                {
                    var orderStatusUpdateCommand =
                        new OrderStatusUpdateCommand(messageAdapter.GetPayloadIn());

                    await new OrderStatusUpdateCommandHandler(ServiceProvider, ExecutingRequestContextAdapter).Handle(
                        orderStatusUpdateCommand);
                    
                    processingStates |= MessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= MessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Order status updated",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}