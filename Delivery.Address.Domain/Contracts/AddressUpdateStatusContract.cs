using System.Runtime.Serialization;

namespace Delivery.Address.Domain.Contracts
{
    [DataContract]
    public class AddressUpdateStatusContract
    {
        [DataMember]
        public bool AddressUpdated { get; set; }
    }
}