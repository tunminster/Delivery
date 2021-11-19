using System.Runtime.Serialization;

namespace Delivery.Customer.Domain.Contracts.V1.Enums
{
    public enum OrderNotificationFilter
    {
        [EnumMember] None = 0,
        [EnumMember] Accept = 1,
        [EnumMember] Arrived = 2,
        [EnumMember] Delivered = 3,
        [EnumMember] ReadyToDeliver = 4
        
    }
}