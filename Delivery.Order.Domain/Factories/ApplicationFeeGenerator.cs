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
            
            return (int)(Math.Round(((decimal)OrderConstant.CustomerApplicationServiceRate / 100) * subTotalAmount)) ;
        }

        public static int GenerateDeliveryFees(int radius)
        {
            var deliverfee = 0;
            
            
            
            if (radius < 1000)
                deliverfee = 199;
            else if (radius is >= 1000 and < 2000)
                deliverfee = 299;
            else if (radius is >= 2000 and < 3000)
                deliverfee = 399;
            else if (radius is >= 3000 and < 4000)
                deliverfee = 499;
            else if (radius is >= 4000 and < 5000)
                deliverfee = 599;
            else if (radius is >= 5000 and < 6000)
                deliverfee = 799;
            else if (radius is >= 6000 and < 7000)
                deliverfee = 899;
            else if (radius is >= 7000 and < 8000)
                deliverfee = 999;
            else if (radius is >= 9000 and < 10000)
                deliverfee = 1099;
            
            return deliverfee > 0 ? deliverfee : throw new ArgumentOutOfRangeException(nameof(radius));
        }
        
        public static int BusinessServiceFees(int subTotalAmount, int serviceRate)
        {
            return (int)(Math.Round(((decimal)serviceRate / 100) * subTotalAmount)) ;
        }
    }
}