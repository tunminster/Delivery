using System.Runtime.Serialization;

namespace Delivery.StripePayment.Domain.Contracts.Models
{
    [DataContract]
    public class StripeAccountContract 
    {
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public string Shard { get; set; }
    }
}