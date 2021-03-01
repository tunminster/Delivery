using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Enum
{
    [DataContract]
    public enum PaymentStatusEnum
    {
        [DataMember]None = 0,
        [DataMember]InProgress = 1,
        [DataMember]Success = 2,
        [DataMember]Failed = 3
    }
}