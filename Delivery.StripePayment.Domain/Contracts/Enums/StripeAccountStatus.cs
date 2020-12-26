using System.Runtime.Serialization;

namespace Delivery.StripePayment.Domain.Contracts.Enums
{
    [DataContract]
    public enum StripeAccountStatus
    {
        [EnumMember] None = 0,
        [EnumMember] Created = 1,
        [EnumMember] Updated =2
    }
}