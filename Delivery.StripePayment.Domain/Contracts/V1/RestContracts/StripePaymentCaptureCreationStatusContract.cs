using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.StripePayment.Domain.Contracts.Enums;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents status of stripe capture creation
    /// </summary>
    [DataContract]
    public class StripePaymentCaptureCreationStatusContract
    {
        /// <summary>
        ///  'succeeded' status if the payment is successful.
        ///  'requires_action' status if the payment needs extra 3D secure confirmation
        ///  'requires_capture' status if the payment needs manual capture request
        /// </summary>
        [DataMember]
        public string PaymentStatus { get; set; }
        
        [DataMember]
        public string PaymentResponseMessage { get; set; }
        
        [DataMember]
        public bool Captured { get; set; }
        
        /// <summary>
        ///  It explains what require to be done in order to complete this payment.
        /// </summary>
        [DataMember]
        public string NextAction { get; set; }
        
        /// <summary>
        ///  Currency that used in the payment captured.
        /// </summary>
        [DataMember]
        public string Currency { get; set; }
        
        [DataMember]
        public string CaptureMethod { get; set; }
        
        [DataMember]
        public long? AmountCaptured { get; set; }
        
        [DataMember]
        public long? ApplicationFeeAmount { get; set; }
        
        [DataMember]
        public string PaymentIntent { get; set; }
        
        [DataMember]
        public string PaymentMethod { get; set; }
        
        [DataMember]
        public string FailureCode { get; set; }
        
        [DataMember]
        public string FailureMessage { get; set; }
        
        [DataMember]
        public string ReceiptNumber { get; set; }
        
        [DataMember]
        public string ReceiptUrl { get; set; }
        
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public DateTimeOffset Created { get; set; }
        
        [DataMember]
        public bool LiveMode { get; set; }
        

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(PaymentStatus)} : {PaymentStatus.Format()};";
        }
    }
}