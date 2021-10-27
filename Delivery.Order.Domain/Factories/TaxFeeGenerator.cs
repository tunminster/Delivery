using System;

namespace Delivery.Order.Domain.Factories
{
    public static class TaxFeeGenerator
    {
        public static int GenerateTaxFees(int subTotalAmount, int taxRate)
        {
            return (int)(Math.Round(((decimal)taxRate / 100) * subTotalAmount));
        }
    }
}