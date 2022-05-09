using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreGeoUpdates;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreGeoUpdate;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Handlers.MessageHandlers.StoreGeoUpdates
{
    public class StoreGeoUpdateMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public StoreGeoUpdateMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(StoreGeoUpdateMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StoreGeoUpdateContract, StoreGeoUpdateStatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.PersistOrder))
                {
                    var storeGeoUpdateCommand =
                        new StoreGeoUpdateCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());
                    
                    await new StoreGeoUpdateCommandHandler(ServiceProvider, ExecutingRequestContextAdapter).HandleAsync(
                        storeGeoUpdateCommand);
                    
                    processingStates |= MessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= MessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Store Geo updated",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}