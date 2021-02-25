using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts.RestContracts.OrderDetails
{
    [DataContract]
    public class OrderDetailsContract
    {
        [DataMember]
        public string EstimatedCookingTime { get; set; }
        
        [DataMember]
        public string StoreName { get; set; }
    }
}