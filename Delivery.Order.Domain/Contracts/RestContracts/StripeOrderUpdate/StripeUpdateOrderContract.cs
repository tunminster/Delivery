using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;
using Delivery.Order.Domain.Enum;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate
{
    [DataContract]
    public class StripeUpdateOrderContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public OrderStatus OrderStatus { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(OrderStatus)}: {OrderStatus.Format()}";
        }
    }
}