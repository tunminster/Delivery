using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Enums;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement
{
    public class ShopOrderStatusMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopOrderStatusMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) 
            : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(ShopOrderStatusMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<ShopOrderStatusCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var shopOrderStatusCommand =
                        new ShopOrderStatusCommand(messageAdapter.GetPayloadIn());

                    var shopOrderStatusContract = new ShopOrderStatusContract();
                    if (!processingStates.HasFlag(MessageProcessingStates.Persisted))
                    {
                        shopOrderStatusContract = await new ShopOrderStatusCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                            .Handle(shopOrderStatusCommand);
                        
                        processingStates |= MessageProcessingStates.Persisted;
                    }

                    if (shopOrderStatusContract.Status &&
                        shopOrderStatusContract.OrderStatus == OrderStatus.Accepted)
                    {
                        var shopOrderDriverRequestCommand = new ShopOrderDriverRequestCommand
                        (
                            new ShopOrderDriverRequestContract
                            {
                                OrderId = shopOrderStatusContract.OrderId
                            }
                        );

                        await new ShopOrderDriverRequestCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                            .Handle(shopOrderDriverRequestCommand);
                    }
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Shop application persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}