using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate
{
    [DataContract]
    public class StripeOrderUpdateStatusContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public DateTimeOffset UpdatedDateTime { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(UpdatedDateTime)} : {UpdatedDateTime.Format()}";

        }
    }
}