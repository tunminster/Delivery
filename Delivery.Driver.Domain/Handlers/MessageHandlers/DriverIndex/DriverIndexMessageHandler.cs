using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverIndex;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverIndex;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverIndex
{
    public class DriverIndexMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverIndexMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverIndexMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverIndexCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var driverIndexCommand =
                        new DriverIndexCommand(messageAdapter.GetPayloadIn().DriverId);

                    await new DriverIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(driverIndexCommand);
                    
                    // complete
                    processingStates |= MessageProcessingStates.Processed;
                }
                
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