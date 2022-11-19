namespace Delivery.Order.Domain.Constants
{
    public static class OrderConstant
    {
        public const int DefaultPreparationTime = 10;
        public const int DefaultPickupMinutes = 20;

        /// <summary>
        ///  Default business application service rate eg: 5%
        /// </summary>
        public const int BusinessApplicationServiceRate = 5;
        
        /// <summary>
        ///  Default customer application service rate eg: 5%
        /// </summary>
        public const int CustomerApplicationServiceRate = 5;

        /// <summary>
        ///  Stripe transaction fees for every card payment
        ///  EG: 2.9%
        /// </summary>
        public const double StripeTransactionFees = 2.9;

        /// <summary>
        ///  Stripe additional transaction fees
        ///  EG: 30 cents
        /// </summary>
        public const int StripeTransactionPlusFees = 30;





    }
}