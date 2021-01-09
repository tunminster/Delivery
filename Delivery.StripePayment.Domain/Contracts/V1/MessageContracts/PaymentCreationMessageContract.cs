using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;

namespace Delivery.StripePayment.Domain.Contracts.V1.MessageContracts
{
    [DataContract]
    public class PaymentCreationMessageContract : AuditableResponseMessage<StripePaymentCreationContract, StripePaymentCreationStatusContract>
    {
        
    }
}