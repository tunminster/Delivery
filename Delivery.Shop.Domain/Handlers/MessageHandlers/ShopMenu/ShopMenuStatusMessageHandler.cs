using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopMenu;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopMenu;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopMenu
{
    public class ShopMenuStatusMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopMenuStatusMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(ShopMenuStatusMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<ShopMenuStatusCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var shopOrderStatusCommand =
                        new ShopMenuStatusCommand(messageAdapter.GetPayloadIn());
                    
                    await new ShopMenuStatusCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(shopOrderStatusCommand);
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
                
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}