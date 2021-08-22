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
        [EnumMember] Processed = 2,
        [EnumMember] Indexed = 4
    }
}