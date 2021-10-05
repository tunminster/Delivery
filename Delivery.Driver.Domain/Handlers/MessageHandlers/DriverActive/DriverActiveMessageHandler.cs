using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverActive;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverElasticSearch;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverActive
{
    public class DriverActiveMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverActiveMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverActiveMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverActiveCreationContract, DriverActiveStatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    var driverActiveCommand = new DriverActiveCommand(messageAdapter.GetPayloadIn());

                    var driverActiveStatusContract = await new DriverActiveCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverActiveCommand);
                    
                    var driverIndexCommand =
                        new DriverIndexCommand(driverActiveStatusContract.DriverId);

                    await new DriverIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverIndexCommand);
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