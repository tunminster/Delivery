using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.RestContracts
{
    [DataContract]
    public class OrderCreationStatusContract
    {
        [DataMember]
        public string OrderId { get; set; }
        
        [DataMember]
        public string Status { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(Status)} : {Status.Format()}";

        }
    }
}