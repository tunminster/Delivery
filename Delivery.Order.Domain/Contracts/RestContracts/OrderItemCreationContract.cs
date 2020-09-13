using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts.RestContracts
{
    [DataContract]
    public class OrderItemCreationContract
    {
        [DataMember]
        public int ProductId { get; set;}
        
        [DataMember]
        public int Count { get; set; }
    }
}