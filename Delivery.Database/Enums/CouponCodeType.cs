using System.Runtime.Serialization;

namespace Delivery.Database.Enums
{
    public enum CouponCodeType
    {
        [EnumMember] None = 0,
        [EnumMember] FixedAmountDiscount = 1,
        [EnumMember] PercentageAmountDiscount = 2
    }
}