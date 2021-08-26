using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;

namespace Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopCreation
{
    public class ShopCreationMessageContract : AuditableResponseMessage<ShopCreationContract, ShopCreationStatusContract>
    {
    }
}