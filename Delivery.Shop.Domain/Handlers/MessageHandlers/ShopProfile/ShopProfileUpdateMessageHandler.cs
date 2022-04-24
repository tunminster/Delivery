using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopProfile;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopProfile;

namespace Delivery.Shop.Domain.Handlers.MessageHandlers.ShopProfile
{
    public class ShopProfileUpdateMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public ShopProfileUpdateMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }
        
        public async Task HandleMessageAsync(ShopProfileMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<ShopProfileCreationContract, StatusContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var shopProfileUpdateCommand =
                        new ShopProfileUpdateCommand(messageAdapter.GetPayloadIn());

                    await new ShopProfileUpdateCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .Handle(shopProfileUpdateCommand);
                    
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