using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum DriverOrderStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Accepted= 1,
        [EnumMember] Rejected = 2,
        [EnumMember] InProgress = 3,
        [EnumMember] Complete = 4,
        [EnumMember] SystemRejected = 5
    }
}