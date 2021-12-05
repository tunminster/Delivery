using System.Runtime.Serialization;

namespace Delivery.Azure.Library.NotificationHub.Contracts.Enums
{
    [DataContract]
    public enum PushNotificationType
    {
        [EnumMember] None = 0,
        [EnumMember] DeliveryRequest = 1,
        [EnumMember] ShopNewOrder = 2,
        [EnumMember] OrderCompleted = 3,
        [EnumMember] CustomerOrderArrived = 4,
        [EnumMember] CustomerOrderAccepted = 5,
        [EnumMember] CustomerDelivered = 6,
        [EnumMember] DeliveryRejected = 7
    }
}