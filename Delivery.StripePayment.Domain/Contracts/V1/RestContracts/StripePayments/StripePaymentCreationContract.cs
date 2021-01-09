using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments
{
    /// <summary>
    ///  A contract represents to create a stripe payment 
    /// </summary>
    [DataContract]
    public class StripePaymentCreationContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public string StripePaymentIntentId { get; set; }
        
        [DataMember]
        public string StripePaymentMethodId { get; set; }
        
        [DataMember]
        public string PaymentStatus { get; set; }
        
        [DataMember]
        public bool Captured { get; set; }
        
        [DataMember]
        public long? AmountCaptured { get; set; }
        
        [DataMember]
        public string FailureCode { get; set; }
        
        [DataMember]
        public string FailureMessage { get; set; }
        
        [DataMember]
        public string ReceiptUrl { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(StripePaymentIntentId)}: {StripePaymentIntentId.Format()}," +
                   $"{nameof(PaymentStatus)}: {PaymentStatus.Format()};";
        }
    }
}