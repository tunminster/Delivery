using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.RestContracts.OrderDetails
{
    [DataContract]
    public class OrderDetailsContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public string EstimatedCookingTime { get; set; }
        
        [DataMember]
        public string StoreName { get; set; }
        
        [DataMember]
        public string StoreAddress { get; set; }
        
        [DataMember]
        public string DeliveryAddress { get; set; }
        
        [DataMember]
        public string OrderStatus { get; set; }
        
        [DataMember]
        public OrderStatus Status { get; set; }
        
        [DataMember]
        public int SubtotalAmount { get; set; }
        
        [DataMember]
        public int TotalAmount { get; set; }
        
        [DataMember]
        public int DeliveryFees { get; set; }
        
        [DataMember]
        public int TaxFees { get; set; }
        
        [DataMember]
        public int ApplicationFees { get; set; }
        
        [DataMember]
        public string ImageUri { get; set; }
        
        [DataMember]
        public OrderType OrderType { get; set; }
        
        [DataMember]
        public List<OrderItemContract> OrderItems { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(EstimatedCookingTime)}: {EstimatedCookingTime.Format()}," +
                   $"{nameof(OrderStatus)}: {OrderStatus.Format()}," +
                   $"{nameof(TotalAmount)}: {TotalAmount.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(DeliveryAddress)}: {DeliveryAddress.Format()}," +
                   $"{nameof(OrderItems)} : {OrderItems.Format()}";

        }
    }
}