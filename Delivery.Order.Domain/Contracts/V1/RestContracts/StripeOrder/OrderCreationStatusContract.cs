using System;
using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder
{
    [DataContract]
    public class OrderCreationStatusContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public int SubtotalAmount { get; set; }
        
        [DataMember]
        public int TotalAmount { get; set; }
        
        [DataMember]
        public int CustomerApplicationFee { get; set; }
        
        [DataMember]
        public int DeliveryFee { get; set; }
        
        [DataMember]
        public int TaxFee { get; set; }
        
        [DataMember]
        public string CurrencyCode { get; set; }
        
        [DataMember]
        public string PaymentAccountNumber { get; set; }
        
        [DataMember]
        public string StripePaymentIntentId { get; set; }
        
        [DataMember]
        public DateTimeOffset CreatedDateTime { get; set; }
        
        [DataMember]
        public int BusinessApplicationFee { get; set; }
    }
}