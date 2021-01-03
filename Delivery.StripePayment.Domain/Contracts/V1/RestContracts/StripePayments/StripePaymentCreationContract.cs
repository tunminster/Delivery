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
        public string StripeClientSecret { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(StripePaymentIntentId)}: {StripePaymentIntentId.Format()}," +
                   $"{nameof(StripeClientSecret)}: {StripeClientSecret.Format()};";
        }
    }
}