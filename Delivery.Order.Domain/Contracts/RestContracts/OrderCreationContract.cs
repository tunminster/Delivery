#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Order.Domain.Contracts.Interfaces;

namespace Delivery.Order.Domain.Contracts.RestContracts
{
    [DataContract]
    public class OrderCreationContract : IOrderCreationContract
    {
        [DataMember]
        public string? CardHolderName { get; set; }
        
        [DataMember]
        public string? CardNumber { get; set; }
        
        [DataMember]
        public string? Cvc { get; set; }
        
        [DataMember]
        public string? ExpiryMonth { get; set; }
        
        [DataMember]
        public string? ExpiryYear { get; set; }
        
        [DataMember]
        public string? TotalAmount { get; set; }
        
        [DataMember]
        public int CustomerId { get; set; }
        
        [DataMember]
        public List<OrderItemCreationContract> OrderItems { get; set; } = new List<OrderItemCreationContract>();
        
        [DataMember]
        public int ShippingAddressId { get; set; }
        
        [DataMember]
        public bool SaveCard { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}:" +
                   $"{nameof(CardHolderName)} :{CardHolderName.Format()}, " +
                   $"{nameof(CardNumber)} :{CardNumber.Format()}, " +
                   $"{nameof(SaveCard)} : {SaveCard.Format()}";
        }
    }
}