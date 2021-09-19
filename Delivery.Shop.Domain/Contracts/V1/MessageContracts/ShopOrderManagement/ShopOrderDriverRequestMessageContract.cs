using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;

namespace Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement
{
    public class ShopOrderDriverRequestMessageContract : AuditableResponseMessage<ShopOrderDriverRequestContract, StatusContract>
    {
    }
}