using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Order.Domain.Contracts.V1.RestContracts.PushNotification;

namespace Delivery.Order.Domain.Contracts.V1.MessageContracts.PushNotification
{
    public class OrderCreatedPushNotificationMessageContract : AuditableResponseMessage<OrderCreatedPushNotificationRequestContract, StatusContract>
    {
        
    }
}