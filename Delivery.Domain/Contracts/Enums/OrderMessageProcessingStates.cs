using System;
using System.Runtime.Serialization;

namespace Delivery.Domain.Contracts.Enums
{
    [Flags]
    [DataContract]
    public enum OrderMessageProcessingStates
    {
        [EnumMember] None = 0,
        [EnumMember] PersistOrder = 1,
        [EnumMember] Persisted = 2,
        [EnumMember] Indexed = 4,
        [EnumMember] Processed = 8,
    }
}