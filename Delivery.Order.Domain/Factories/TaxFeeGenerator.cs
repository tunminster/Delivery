namespace Delivery.Order.Domain.Factories
{
    public static class TaxFeeGenerator
    {
        public static int GenerateTaxFees(int subTotalAmount, int taxRate)
        {
            return (taxRate % 100 * subTotalAmount);
        }
    }
}