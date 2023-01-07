using Delivery.Database.Entities;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.Coupon;

namespace Delivery.Managements.Domain.Converters
{
    public static class CouponCodeConverter
    {
        public static CouponCode ConvertToCouponCode(
            this CouponManagementCreationContract couponManagementCreationContract)
        {
            var couponCode = new CouponCode
            {
                CouponId = couponManagementCreationContract.CouponId,
                CouponCodeType = couponManagementCreationContract.CouponCodeType,
                DiscountAmount = couponManagementCreationContract.DiscountAmount,
                Name = couponManagementCreationContract.Name,
                PromotionCode = couponManagementCreationContract.PromotionCode,
                MinimumOrderValue = couponManagementCreationContract.MinimumOrderValue,
                RedeemBy = couponManagementCreationContract.RedeemBy,
                NumberOfTimes = couponManagementCreationContract.NumberOfTimes
            };

            return couponCode;
        }
    }
}