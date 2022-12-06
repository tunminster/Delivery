using Delivery.Order.Domain.Constants;

namespace Delivery.Order.Domain.Factories
{
    public static class StripeApplicationFeesAmount
    {
        public static int CalculateStripeApplicationFeeAmount(int subtotal, int applicationFees, int deliveryFee, int deliveryTips, double businessServiceRate)
        {
            return applicationFees + deliveryFee + deliveryTips +
                   ApplicationFeeGenerator.BusinessServiceFees(subtotal, businessServiceRate) + 30;  // stripe charges 2.0 % + 30 cents every transaction
        }

        public static int GetStripeTransactionFees(int subtotalAmount)
        {
            return (int) ((OrderConstant.StripeTransactionFees / 100) * subtotalAmount);
        }
    }
}