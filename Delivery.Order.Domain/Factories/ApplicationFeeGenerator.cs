namespace Delivery.Order.Domain.Factories
{
    public static class ApplicationFeeGenerator
    {
        public static int GeneratorFees(int totalAmount)
        {
            return (int)(0.5 % 100 * totalAmount);
        }
    }
}