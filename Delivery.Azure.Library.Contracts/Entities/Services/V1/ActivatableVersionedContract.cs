using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Contracts.Entities.Services.V1
{
    [DataContract]
    public class ActivatableVersionedContract : VersionedContract
    {
        [DataMember]
        public bool IsActive { get; set; }
    }
}