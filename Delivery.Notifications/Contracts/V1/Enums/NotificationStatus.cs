using System.Runtime.Serialization;

namespace Delivery.Notifications.Contracts.V1.Enums
{
    [DataContract]
    public enum NotificationStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Created = 1,
    }
}