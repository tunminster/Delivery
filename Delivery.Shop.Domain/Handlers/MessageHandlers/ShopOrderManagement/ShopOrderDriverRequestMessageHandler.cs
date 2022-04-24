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
    public class ShopOrderDriverRequestMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopOrderDriverRequestMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) 
            : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(ShopOrderDriverRequestMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<ShopOrderDriverRequestContract, StatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var shopOrderStatusCommand =
                        new ShopOrderDriverRequestCommand(messageAdapter.GetPayloadIn());
                    
                    await new ShopOrderDriverRequestCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(shopOrderStatusCommand);
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Driver requested",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}