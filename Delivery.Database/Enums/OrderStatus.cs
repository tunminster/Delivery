using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum OrderStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Accepted= 1,
        [EnumMember] Rejected = 2,
        [EnumMember] Preparing = 3,
        [EnumMember] Ready = 4,
        [EnumMember] DeliveryOnWay = 5,
        [EnumMember] Completed = 6,
        [EnumMember] NoDeliveryAvailable = 7
    }
}