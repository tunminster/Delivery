using System;
using System.Runtime.Serialization;
using Microsoft.Graph;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrder
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
        public int ApplicationFee { get; set; }
        
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
    }
}