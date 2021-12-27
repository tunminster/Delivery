using System;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Stripe;

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

        public static CouponCodeContract ConvertToCouponCodeContract(this PromotionCode promotionCode)
        {
            var couponCodeContract = new CouponCodeContract
            {
                CouponId = promotionCode.Id,
                CouponCodeType = CouponCodeType.FixedAmountDiscount,
                PromoCode = promotionCode.Code,
                RedeemBy = DateTimeOffset.Parse(promotionCode.Coupon.RedeemBy.ToString()),
                DiscountAmount = int.Parse(promotionCode.Coupon.AmountOff.ToString()),
                MinimumOrderValue = int.Parse(promotionCode.Restrictions.MinimumAmount.ToString()),
                NumberOfTimes = int.Parse(promotionCode.Coupon.TimesRedeemed.ToString())
            };

            return couponCodeContract;
        }
    }
}