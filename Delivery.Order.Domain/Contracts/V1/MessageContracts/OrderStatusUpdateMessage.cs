using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate;

namespace Delivery.Order.Domain.Contracts.V1.MessageContracts
{
    public class OrderStatusUpdateMessage : AuditableResponseMessage<StripeUpdateOrderContract, StripeUpdateOrderStatusContract>
    {
        
    }
}