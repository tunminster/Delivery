using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.CommandHandlers.StripePaymentCreation;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.StripePayment.Domain.Handlers.MessageHandlers
{
    public class PaymentCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public PaymentCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(PaymentCreationMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StripePaymentCreationContract, StripePaymentCreationStatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.PersistOrder))
                {
                    var stripePaymentCreationCommand =
                        new StripePaymentCreationCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());
                    
                    var stripePaymentCreationCommandHandler =
                        new StripePaymentCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter);
                    await stripePaymentCreationCommandHandler.Handle(stripePaymentCreationCommand);

                    processingStates |= OrderMessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= OrderMessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Payment persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
            
        }
    }
}