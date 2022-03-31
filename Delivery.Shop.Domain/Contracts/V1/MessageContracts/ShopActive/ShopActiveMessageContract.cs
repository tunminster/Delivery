using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopActive;

namespace Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopActive;

public class ShopActiveMessageContract : AuditableResponseMessage<ShopActiveCreationContract, StatusContract>
{
    
}