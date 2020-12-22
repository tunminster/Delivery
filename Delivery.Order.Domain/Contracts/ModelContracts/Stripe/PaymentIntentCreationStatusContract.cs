namespace Delivery.Order.Domain.Contracts.ModelContracts.Stripe
{
    public class PaymentIntentCreationStatusContract
    {
        public PaymentIntentCreationStatusContract(string stripeClientSecret)
        {
            StripeClientSecret = stripeClientSecret;
        }
        
        public string StripeClientSecret { get; }
    }
}