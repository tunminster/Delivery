using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCreation;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers
{
    public class DriverCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverCreationMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverCreationContract, DriverCreationStatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Persisted))
                {
                    var driverCreationCommand =
                        new DriverCreationCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());

                    await new DriverCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverCreationCommand);
                    
                    processingStates |= OrderMessageProcessingStates.Persisted;
                }

                if (processingStates.HasFlag(OrderMessageProcessingStates.Persisted) &&
                    !processingStates.HasFlag(OrderMessageProcessingStates.Indexed))
                {
                    var driverIndexCommand =
                        new DriverIndexCommand(messageAdapter.GetPayloadOut().DriverId);

                    await new DriverIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverIndexCommand);
                    
                    processingStates |= OrderMessageProcessingStates.Indexed;
                }
                
                // complete
                processingStates |= OrderMessageProcessingStates.Processed;
                
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Driver application persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());

            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}