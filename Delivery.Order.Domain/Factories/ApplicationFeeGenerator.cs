namespace Delivery.Order.Domain.Factories
{
    public static class ApplicationFeeGenerator
    {
        public static int GeneratorFees(int totalAmount)
        {
            if (totalAmount < 100)
            {
                return 1;
            }
            return (int)(0.01 % 100 * totalAmount);
        }
    }
}