using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Handlers.MessageHandlers.OrderUpdates
{
    public class OrderUpdateMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public OrderUpdateMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(OrderUpdateMessage message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StripeOrderUpdateContract, StripeOrderUpdateStatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.PersistOrder))
                {
                    var orderUpdateCommand =
                        new OrderUpdateCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());
                    
                    var orderUpdateCommandHandler =
                        new OrderUpdateCommandHandler(ServiceProvider, ExecutingRequestContextAdapter);
                    await orderUpdateCommandHandler.Handle(orderUpdateCommand);
                    
                    processingStates |= OrderMessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= OrderMessageProcessingStates.Processed;

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