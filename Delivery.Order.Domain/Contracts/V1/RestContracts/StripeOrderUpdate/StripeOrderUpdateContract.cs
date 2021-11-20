using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate
{
    /// <summary>
    ///  A contract to update an order
    /// </summary>
    [DataContract]
    public class StripeOrderUpdateContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public OrderStatus OrderStatus { get; set; }
        
        [DataMember]
        public string PaymentStatus { get; set; }
        
        [DataMember]
        public string PaymentIntentId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(OrderStatus)}: {OrderStatus.Format()}," +
                   $"{nameof(PaymentStatus)}: {PaymentStatus.Format()}," +
                   $"{nameof(PaymentIntentId)} : {PaymentIntentId.Format()}";

        }
        
        
    }
}