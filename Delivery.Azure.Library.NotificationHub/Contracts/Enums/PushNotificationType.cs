using System.Runtime.Serialization;

namespace Delivery.Azure.Library.NotificationHub.Contracts.Enums
{
    [DataContract]
    public enum PushNotificationType
    {
        [EnumMember] None = 0,
        [EnumMember] DeliveryRequest = 1,
        [EnumMember] ShopNewOrder = 2
    }
}