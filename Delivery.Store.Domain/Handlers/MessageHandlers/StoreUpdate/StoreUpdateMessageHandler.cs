using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreUpdate;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreUpdate;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Handlers.MessageHandlers.StoreUpdate
{
    public class StoreUpdateMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public StoreUpdateMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(StoreUpdateMessage message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StoreUpdateContract, StoreUpdateStatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.PersistOrder))
                {
                    var storeUpdateCommand =
                        new StoreUpdateCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());
                    
                    await new StoreUpdateCommandHandler(ServiceProvider, ExecutingRequestContextAdapter).Handle(
                        storeUpdateCommand);
                    
                    processingStates |= OrderMessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= OrderMessageProcessingStates.Processed;
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Store updated",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}