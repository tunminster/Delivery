using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment
{
    public class DriverAssignmentMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverAssignmentMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverAssignmentMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverAssignmentCreationContract, DriverAssignmentStatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var driverAssignmentCommand =
                        new DriverAssignmentCommand(messageAdapter.GetPayloadIn());
                    
                    await new DriverAssignmentCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverAssignmentCommand);
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Driver assignment persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
                
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}