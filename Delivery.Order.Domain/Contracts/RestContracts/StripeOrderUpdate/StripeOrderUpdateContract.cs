using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate
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
        public string OrderStatus { get; set; }
        
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