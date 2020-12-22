using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Stripe;

namespace Delivery.Order.Domain.Contracts.ModelContracts.Stripe
{
    [DataContract]
    public class PaymentIntentCreationContract
    {
        [DataMember]
        public string PaymentMethod { get; set; }
        
        [DataMember]
        public int Amount { get; set; }
        
        [DataMember]
        public string Currency { get; set; }
        
        [DataMember]
        public int ApplicationFeeAmount { get; set; }
        
        [DataMember]
        public string ConnectedStripeAccountId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(PaymentMethod)}: {PaymentMethod.Format()}," +
                   $"{nameof(Amount)}: {Amount.Format()}," +
                   $"{nameof(Currency)}: {Currency.Format()}," +
                   $"{nameof(ApplicationFeeAmount)}: {ApplicationFeeAmount.Format()}," +
                   $"{nameof(ConnectedStripeAccountId)} : {ConnectedStripeAccountId.Format()}";

        }
    }
}