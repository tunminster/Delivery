using System.Runtime.Serialization;

namespace Delivery.Shop.Domain.Contracts.V1.Enums
{
    public enum ShopPaymentHistoryFilter
    {
        [EnumMember] None = 0,
        [EnumMember] Weekly = 1,
        [EnumMember] Monthly = 2,
        [EnumMember] CurrentWeek = 3
    }
}