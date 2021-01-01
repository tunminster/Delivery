using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Entities.Services.V1;

namespace Delivery.Azure.Library.Sharding.Contracts.V1
{
    [DataContract]
    public class ShardInformation : ActivatableVersionedContract
    {
        [DataMember]
        public string? Key { get; set; }

        [DataMember]
        public string? DisplayName { get; set; }

        [DataMember]
        public int? Ring { get; set; }

        public override string ToString()
        {
            return $"{nameof(ShardInformation)} - {nameof(DisplayName)}: {DisplayName}, {nameof(Key)}: {Key}, {nameof(Ring)}: {Ring}, {nameof(IsActive)}: {IsActive}, {nameof(Version)}: {Version}";
        }
    }
}