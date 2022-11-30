using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptions;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions;
using Delivery.Managements.Domain.Handlers.CommandHandlers;

namespace Delivery.Managements.Domain.Handlers.MessageHandler
{
    public class MeatOptionCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public MeatOptionCreationMessageHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider,
            executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(MeatOptionCreationMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter = new AuditableRequestMessageAdapter<MeatOptionCreationMessageContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var meatOptionCreationCommand = new MeatOptionCreationCommand(messageAdapter.PayloadIn());

                    await new MeatOptionCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(meatOptionCreationCommand);

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