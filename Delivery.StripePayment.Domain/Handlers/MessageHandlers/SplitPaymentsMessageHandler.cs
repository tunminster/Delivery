using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.SplitPayments;
using Delivery.StripePayment.Domain.Handlers.CommandHandlers.SplitPayments;

namespace Delivery.StripePayment.Domain.Handlers.MessageHandlers
{
    public class SplitPaymentsMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public SplitPaymentsMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(SplitPaymentCreationMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<SplitPaymentCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    var splitPaymentCommand =
                        new SplitPaymentCommand(messageAdapter.GetPayloadIn());
                    
                    var splitPaymentCommandHandler =
                        new SplitPaymentCommandHandler(ServiceProvider, ExecutingRequestContextAdapter);
                    await splitPaymentCommandHandler.Handle(splitPaymentCommand);
                }
                
                // complete
                processingStates |= OrderMessageProcessingStates.Processed;
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}