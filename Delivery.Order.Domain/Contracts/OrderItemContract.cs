using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts
{
    [DataContract]
    public class OrderItemContract
    {
        [DataMember]
        public int ProductId { get; set; }
        
        [DataMember]
        public string ProductName { get; set; }
        
        [DataMember]
        public decimal ProductPrice { get; set; }
        
        [DataMember]
        public int Count { get; set; }
    }
}