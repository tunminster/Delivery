using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement
{
    public class ShopOrderIndexMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopOrderIndexMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }
        
        public async Task HandleMessageAsync(ShopOrderIndexMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<ShopOrderIndexCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    var shopOrderIndexCommand =
                        new ShopOrderIndexCommand(messageAdapter.GetPayloadIn().OrderId);

                    if (!processingStates.HasFlag(OrderMessageProcessingStates.Persisted))
                    {
                        await new ShopOrderIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                            .Handle(shopOrderIndexCommand);
                        
                        processingStates |= OrderMessageProcessingStates.Processed;
                    }
                }
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Shop order indexed",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}