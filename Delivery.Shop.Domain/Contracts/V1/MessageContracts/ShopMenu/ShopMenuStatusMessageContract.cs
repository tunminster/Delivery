using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu;

namespace Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopMenu
{
    public class ShopMenuStatusMessageContract : AuditableResponseMessage<ShopMenuStatusCreationContract, StatusContract>
    {
    }
}