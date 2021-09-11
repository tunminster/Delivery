using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts
{
    [DataContract]
    public class OrderContract
    {
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public int CustomerId { get; set; }
        
        [DataMember]
        public decimal TotalAmount { get; set; }
        
        [DataMember]
        public OrderType OrderType { get; set; }
        
        [DataMember]
        public string ImageUri { get; set; }
        
        [DataMember]
        public OrderStatus Status { get; set; }
        
        [DataMember]
        public string StoreName { get; set; }
        
        [DataMember]
        public DateTimeOffset DateCreated { get; set; }
        
        [DataMember]
        public string DeliveryAddress { get; set; }
        
        [DataMember]
        public List<OrderItemContract> OrderItems { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}," +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(Status)}: {Status.Format()}," +
                   $"{nameof(TotalAmount)}: {TotalAmount.Format()}," +
                   $"{nameof(OrderType)}: {OrderType.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(DeliveryAddress)}: {DeliveryAddress.Format()}," +
                   $"{nameof(DateCreated)}: {DateCreated.Format()}," +
                   $"{nameof(OrderItems)} : {OrderItems.Format()}";

        }
    }
    
}