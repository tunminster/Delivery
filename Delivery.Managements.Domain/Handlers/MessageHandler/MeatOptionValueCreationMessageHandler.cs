using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptionValues;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptionValues;
using Delivery.Managements.Domain.Handlers.CommandHandlers;

namespace Delivery.Managements.Domain.Handlers.MessageHandler
{
    public class MeatOptionValueCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public MeatOptionValueCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(MeatOptionValueCreationMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter = new AuditableRequestMessageAdapter<MeatOptionValueCreationContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var meatOptionValueCreationCommand = new MeatOptionValueCreationCommand(messageAdapter.PayloadIn());

                    await new MeatOptionValueCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(meatOptionValueCreationCommand);
                
                    processingStates |= MessageProcessingStates.Processed;
                }
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}