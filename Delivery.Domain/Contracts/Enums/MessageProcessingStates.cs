using System;
using System.Runtime.Serialization;

namespace Delivery.Domain.Contracts.Enums
{
    [Flags]
    [DataContract]
    public enum MessageProcessingStates
    {
        [EnumMember] None = 0,
        [EnumMember] PersistOrder = 1,
        [EnumMember] Persisted = 2,
        [EnumMember] Indexed = 4,
        [EnumMember] OnBoardingLinkCreated = 8,
        [EnumMember] NotificationSent = 16,
        [EnumMember] Processed = 32
        
    }
}