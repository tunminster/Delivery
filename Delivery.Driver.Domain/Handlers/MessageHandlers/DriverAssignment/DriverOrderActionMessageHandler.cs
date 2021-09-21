using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment
{
    public class DriverOrderActionMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverOrderActionMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }
        
        public async Task HandleMessageAsync(DriverOrderActionMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverOrderActionContract, StatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    var driverOrderActionCommand =
                        new DriverOrderActionCommand(messageAdapter.GetPayloadIn());
                    
                    await new DriverOrderActionCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverOrderActionCommand);
                    
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