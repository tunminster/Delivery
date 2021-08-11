using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum DriverOrderStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Accepted= 1,
        [EnumMember] Rejected = 2
    }
}