using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.MessageContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexing;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreCreation;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.MessageHandlers.StoreIndexing
{
    public class StoreIndexingMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public StoreIndexingMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }
        
        public async Task HandleMessageAsync(StoreIndexingMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<StoreIndexCreationContract, StoreIndexStatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.PersistOrder))
                {
                    var storeIndexCommand =
                        new StoreIndexCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());

                    await new StoreIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter).Handle(
                        storeIndexCommand);
                    
                    processingStates |= MessageProcessingStates.PersistOrder;
                }
                
                // complete
                processingStates |= MessageProcessingStates.Processed;

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Store indexed",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
                
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}