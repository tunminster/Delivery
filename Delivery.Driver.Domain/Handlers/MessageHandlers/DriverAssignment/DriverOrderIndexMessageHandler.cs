using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment
{
    public class DriverOrderIndexMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverOrderIndexMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverOrderIndexMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverOrderIndexCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    var driverOrderIndexCommand =
                        new DriverOrderIndexCommand(messageAdapter.GetPayloadIn());
                    
                    await new DriverOrderIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverOrderIndexCommand);
                    
                    processingStates |= OrderMessageProcessingStates.Processed;
                }
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}