using System.Runtime.Serialization;

namespace Delivery.Address.Domain.Contracts
{
    [DataContract]
    public class AddressDeleteStatusContract
    {
        [DataMember]
        public bool AddressDeleted { get; set; }
    }
}