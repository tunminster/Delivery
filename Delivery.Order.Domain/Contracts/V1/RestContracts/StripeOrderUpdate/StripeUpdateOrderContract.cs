using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate
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