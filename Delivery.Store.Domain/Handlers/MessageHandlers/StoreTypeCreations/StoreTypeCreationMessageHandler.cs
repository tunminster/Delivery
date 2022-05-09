using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreTypeCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreTypeCreation;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Handlers.MessageHandlers.StoreTypeCreations
{
    public class StoreTypeCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public StoreTypeCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }
        
        public async Task HandleMessageAsync(StoreTypeCreationMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StoreTypeCreationContract, StoreTypeCreationStatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.PersistOrder))
                {
                    var storeTypeCreationCommand =
                        new StoreTypeCreationCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());

                    await new StoreTypeCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter).HandleAsync(
                        storeTypeCreationCommand);
                    
                    processingStates |= MessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= MessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Store type persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
                
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}