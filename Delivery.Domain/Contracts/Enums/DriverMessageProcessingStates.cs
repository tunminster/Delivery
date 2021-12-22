using System;
using System.Runtime.Serialization;

namespace Delivery.Domain.Contracts.Enums
{
    [Flags]
    public enum DriverMessageProcessingStates
    {
        [EnumMember] None = 0,
        [EnumMember] PersistDriver = 1,
        [EnumMember] Persisted = 2,
        [EnumMember] Indexed = 4,
        [EnumMember] Processed = 8
    }
}