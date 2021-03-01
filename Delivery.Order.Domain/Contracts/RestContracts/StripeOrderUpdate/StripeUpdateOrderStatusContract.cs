using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate
{
    [DataContract]
    public class StripeUpdateOrderStatusContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public DateTimeOffset UpdatedDate { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(UpdatedDate)}: {UpdatedDate.Format()}";
        }
    }
}