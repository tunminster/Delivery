using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum OrderPaymentStatus
    {
        [EnumMember] None = 0,
        [EnumMember] InProgress = 1,
        [EnumMember] Succeed = 2,
        [EnumMember] Failed = 3
    }
}