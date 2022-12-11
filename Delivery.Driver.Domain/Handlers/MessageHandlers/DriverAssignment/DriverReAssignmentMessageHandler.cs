using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment
{

    public class DriverReAssignmentMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverReAssignmentMessageHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider,
            executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverReAssignmentMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableRequestMessageAdapter<DriverReAssignmentCreationContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var driverReAssignmentCommand =
                        new DriverReAssignmentCommand(messageAdapter.PayloadIn());

                    await new DriverReAssignmentCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(driverReAssignmentCommand);

                    processingStates |= MessageProcessingStates.Processed;
                }

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric(
                    "Driver Re assignment requested",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());

            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}