using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopActive;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopActive;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopActive;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopMenu;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopActive
{
    public class ShopActiveMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopActiveMessageHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider,
            executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(ShopActiveMessageContract message,
            OrderMessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<ShopActiveCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(OrderMessageProcessingStates.Processed))
                {
                    var shopActiveCommand =
                        new ShopActiveCommand(messageAdapter.GetPayloadIn());

                    await new ShopActiveCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(shopActiveCommand);

                    processingStates |= OrderMessageProcessingStates.Processed;
                }

            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}