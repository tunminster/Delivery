using Delivery.Database.Entities;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;

namespace Delivery.StripePayment.Domain.Converters.CouponPayments
{
    public static class CouponCodeContractConverter
    {
        public static CouponCodeContract ConvertToCouponCodeContract(this Database.Entities.CouponCode couponCode)
        {
            var couponCodeContract = new CouponCodeContract
            {
                CouponId = couponCode.CouponId,
                CouponCodeType = couponCode.CouponCodeType,
                PromoCode = couponCode.PromotionCode,
                RedeemBy = couponCode.RedeemBy,
                NumberOfTimes = couponCode.NumberOfTimes,
                MinimumOrderValue = couponCode.MinimumOrderValue,
                DiscountAmount = couponCode.DiscountAmount
            };

            return couponCodeContract;
        }
    }
}