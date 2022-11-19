using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Messaging.Messages.V1;

namespace Delivery.Order.Domain.Contracts.V1.MessageContracts.CouponPayment
{
    public class CouponPaymentCreationMessage : AuditableRequestMessage<CouponPaymentCreationMessageContract>
    {
    }
}