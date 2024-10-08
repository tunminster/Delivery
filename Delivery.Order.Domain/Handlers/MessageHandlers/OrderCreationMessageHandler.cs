using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder;
using Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderCreation;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Handlers.MessageHandlers
{
    public class OrderCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public OrderCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(OrderCreationMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StripeOrderCreationContract, OrderCreationStatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.PersistOrder))
                {
                    var orderCreationCommand =
                        new OrderCreationCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());
                    
                    // persist order
                    var stripeOrderCreationCommandHandler =
                        new OrderCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter);
                    var orderCreationStatusContract = await stripeOrderCreationCommandHandler.HandleAsync(orderCreationCommand);
                    

                    processingStates |= MessageProcessingStates.PersistOrder;
                }

                // complete
                processingStates |= MessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Order persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}