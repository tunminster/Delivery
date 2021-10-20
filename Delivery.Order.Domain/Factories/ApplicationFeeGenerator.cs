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
                < 1000 => 199,
                1000 => 299,
                2000 => 399,
                3000 => 499,
                4000 => 599,
                5000 => 699,
                > 5000 => 799
            };
        }
        
        public static int BusinessServiceFees(int subTotalAmount, int serviceRate)
        {
            return (serviceRate / 100 * subTotalAmount);
        }
    }
}