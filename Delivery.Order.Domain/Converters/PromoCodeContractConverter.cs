using Delivery.Order.Domain.Contracts.V1.RestContracts.Promotion;

namespace Delivery.Order.Domain.Converters
{
    public static class PromoCodeContractConverter
    {
        public static OrderPromotionDiscountContract CovertToOrderPromotionCodeContract(
            this Database.Entities.CouponCode couponCode)
        {
            var orderPromotionDiscountContract = new OrderPromotionDiscountContract
            {
                PromotionId = couponCode.CouponId,
                PromotionCode = couponCode.PromotionCode,
                PromotionDiscountAmount = couponCode.DiscountAmount
            };

            return orderPromotionDiscountContract;
        }
    }
}