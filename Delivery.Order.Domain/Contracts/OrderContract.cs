using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts
{
    [DataContract]
    public class OrderContract
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public int CustomerId { get; set; }
        
        [DataMember]
        public string OrderStatus { get; set; }
        
        [DataMember]
        public decimal TotalAmount { get; set; }
        
        [DataMember]
        public OrderType OrderType { get; set; }
        
        [DataMember]
        public DateTime DateCreated { get; set; }
        
        [DataMember]
        public List<OrderItemContract> OrderItems { get; set; }
    }
    
}