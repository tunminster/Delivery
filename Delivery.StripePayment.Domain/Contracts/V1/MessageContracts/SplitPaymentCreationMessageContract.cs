using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.SplitPayments;

namespace Delivery.StripePayment.Domain.Contracts.V1.MessageContracts
{
    public class SplitPaymentCreationMessageContract : AuditableResponseMessage<SplitPaymentCreationContract, StatusContract>
    {
    }
}