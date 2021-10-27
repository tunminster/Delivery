using System;
using Delivery.Order.Domain.Constants;

namespace Delivery.Order.Domain.Factories
{
    public static class ApplicationFeeGenerator
    {
        public static int GeneratorFees(int subTotalAmount)
        {
            if (subTotalAmount < 2000)
            {
                return 100;
            }
            return (OrderConstant.CustomerApplicationServiceRate / 100 * subTotalAmount) * 100;
        }

        public static int GenerateDeliveryFees(int radius)
        {
            if (radius < 1000)
                return 199;
            if (radius >= 1000)
                return 299;
            if (radius >= 2000)
                return 399;
            if (radius >= 3000)
                return 499;
            if (radius >= 4000)
                return 599;
            if (radius == 5000)
                return 699;
            if (radius > 5000)
                return 799;
            throw new ArgumentOutOfRangeException(nameof(radius));
        }
        
        public static int BusinessServiceFees(int subTotalAmount, int serviceRate)
        {
            return (int)(Math.Round((double)(serviceRate / 100 * subTotalAmount), MidpointRounding.ToEven)) * 100;
        }
    }
}