using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;
using Microsoft.Graph;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrder
{
    /// <summary>
    ///  A contract to receive an order
    /// </summary>
    [DataContract]
    public class StripeOrderCreationContract
    {
        [DataMember]
        public int CustomerId { get; set; }
        
        [DataMember]
        public List<OrderItemCreationContract> OrderItems { get; set; } = new();
        
        [DataMember]
        public int? ShippingAddressId { get; set; }
        
        [DataMember]
        public OrderType OrderType { get; set; }
        
        [DataMember]
        public decimal Discount { get; set; }
        
        [DataMember]
        public string StoreId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(OrderItems)}: {OrderItems.Format()}," +
                   $"{nameof(ShippingAddressId)}: {ShippingAddressId.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(Discount)} : {Discount.Format()}";

        }
        
       
    }
}