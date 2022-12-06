using Delivery.Order.Domain.Constants;

namespace Delivery.Order.Domain.Factories
{
    public static class StripeApplicationFeesAmount
    {
        public static int CalculateStripeApplicationFeeAmount(int subtotal, int applicationFees, int deliveryFee, int deliveryTips, int taxFee, double businessServiceRate)
        {
            var totalApplicationFees = applicationFees + deliveryFee + deliveryTips + taxFee +
                   ApplicationFeeGenerator.BusinessServiceFees(subtotal, businessServiceRate);  // stripe charges 2.0 % ( you don't need to substract 30 cents every transaction. stripe will do)

            // minus stripe charges 2.9 % from application fees. Meaning give this fees to client.
            var applicationFeesStripeCharges = (int)(totalApplicationFees * (OrderConstant.StripeTransactionFees / 100));
            return totalApplicationFees - applicationFeesStripeCharges;

        }

        public static int GetStripeTransactionFees(int subtotalAmount)
        {
            return (int) ((OrderConstant.StripeTransactionFees / 100) * subtotalAmount);
        }
    }
}