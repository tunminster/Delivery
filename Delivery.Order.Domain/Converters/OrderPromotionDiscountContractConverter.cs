using Delivery.Order.Domain.Contracts.V1.RestContracts.Promotion;

namespace Delivery.Order.Domain.Converters
{
    public static class OrderPromotionDiscountContractConverter
    {
        public static OrderPromotionDiscountContract ConvertToOrderPromotionDiscountContract(
            this Database.Entities.CouponCodeCustomer couponCodeCustomer)
        {
            var orderPromotionDiscountContract = new OrderPromotionDiscountContract
            {
                PromotionId = couponCodeCustomer.CouponCode.CouponId,
                PromotionCode = couponCodeCustomer.PromotionCode,
                PromotionDiscountAmount = couponCodeCustomer.CouponCode.DiscountAmount
            };

            return orderPromotionDiscountContract;
        }
        
        public static OrderPromotionDiscountContract ConvertToOrderPromotionDiscountContract(
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