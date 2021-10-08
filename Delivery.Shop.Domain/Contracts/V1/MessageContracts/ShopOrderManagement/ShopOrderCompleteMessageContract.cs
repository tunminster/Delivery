using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement
{
    public class ShopOrderCompleteMessageContract : AuditableResponseMessage<EntityUpdateContract, StatusContract>
    {
        
    }
}