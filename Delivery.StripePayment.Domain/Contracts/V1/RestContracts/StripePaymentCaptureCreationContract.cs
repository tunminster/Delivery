using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    
    /// <summary>
    ///  A contract represents to capture stripe payment.
    /// </summary>
    [DataContract]
    public class StripePaymentCaptureCreationContract
    {
        /// <summary>
        ///  The id that stripe provides by calling the create payment id
        /// </summary>
        [DataMember]
        public string StripePaymentIntentId { get; set; }
        
        /// <summary>
        ///  The id that stripe provides by calling the create payment type
        /// </summary>
        [DataMember]
        public string StripePaymentMethodId { get; set; }
        
        /// <summary>
        ///  The finger print that stripe provides by calling the create payment type
        /// </summary>
        [DataMember]
        public string StripeFingerPrint { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StripePaymentIntentId)} : {StripePaymentIntentId.Format()}," +
                   $"{nameof(StripePaymentMethodId)} : {StripePaymentMethodId.Format()}," +
                   $"{nameof(StripeFingerPrint)} : {StripeFingerPrint.Format()};";
        }
    }
}