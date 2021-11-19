using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate
{
    [DataContract]
    public class StripeOrderUpdateStatusContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public DateTimeOffset UpdatedDateTime { get; set; }
        
        [DataMember]
        public OrderPaymentStatus PaymentStatusEnum { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(UpdatedDateTime)} : {UpdatedDateTime.Format()}";

        }
    }
}