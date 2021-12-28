namespace Delivery.Order.Domain.Factories
{
    public static class StripeApplicationFeesAmount
    {
        public static int CalculateStripeApplicationFeeAmount(int subtotal, int applicationFees, int deliveryFee, int deliveryTips, int businessServiceRate)
        {
            return applicationFees + deliveryFee +
                   ApplicationFeeGenerator.BusinessServiceFees(subtotal, businessServiceRate);
        }
    }
}