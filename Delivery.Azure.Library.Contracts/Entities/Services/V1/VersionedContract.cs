using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;

namespace Delivery.Azure.Library.Contracts.Entities.Services.V1
{
    [DataContract]
    public class VersionedContract : IVersionedContract
    {
        [DataMember]
        public virtual int Version { get; set; } = 1;
    }
}