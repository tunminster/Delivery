using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;

namespace Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopProfile
{
    public class ShopProfileMessageContract : AuditableResponseMessage<ShopProfileCreationContract, StatusContract>
    {
        
    }
}