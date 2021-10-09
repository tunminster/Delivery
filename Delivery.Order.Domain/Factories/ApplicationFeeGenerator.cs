using Delivery.Order.Domain.Constants;

namespace Delivery.Order.Domain.Factories
{
    public static class ApplicationFeeGenerator
    {
        public static int GeneratorFees(int subTotalAmount)
        {
            if (subTotalAmount < 1000)
            {
                return 100;
            }
            return (int)(OrderConstant.CustomerApplicationServiceRate / 100 * subTotalAmount);
        }

        public static int GenerateDeliveryFees(int radius)
        {
            return radius switch
            {
                < 1 => 199,
                1 => 299,
                2 => 399,
                3 => 499,
                4 => 599,
                5 => 699,
                > 5 => 799
            };
        }
        
        public static int BusinessServiceFees(int subTotalAmount, int serviceRate)
        {
            return (serviceRate / 100 * subTotalAmount);
        }
    }
}