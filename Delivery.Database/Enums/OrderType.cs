using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    [DataContract]
    public enum OrderType
    {
        [EnumMember] None = 0,
        [EnumMember] PickupAt = 1,
        [EnumMember] DeliverTo = 2
    }
}