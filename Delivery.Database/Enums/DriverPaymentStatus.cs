using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum DriverPaymentStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Succeed = 1,
        [EnumMember] Failed = 2
    }
}