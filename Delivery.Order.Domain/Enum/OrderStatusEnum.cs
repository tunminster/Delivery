using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Enum
{
    [DataContract]
    public enum OrderStatusEnum
    {
        [DataMember]None =0,
        [DataMember]Preparing = 1,
        [DataMember]ReadyToPickUp = 2,
        [DataMember]ReadyToDeliver = 3,
        [DataMember]DeliveryOnWay = 4,
        [DataMember]PickedUp = 5,
        [DataMember]Delivered = 6
    }
}