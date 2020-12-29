namespace Delivery.Order.Domain.Contracts.ModelContracts.Stripe
{
    public class PaymentIntentCreationStatusContract
    {
        public PaymentIntentCreationStatusContract(string stripePaymentIntentId, string stripeClientSecret)
        {
            StripePaymentIntentId = stripePaymentIntentId;
            StripeClientSecret = stripeClientSecret;
        }
        
        public string StripePaymentIntentId { get;  }
        public string StripeClientSecret { get; }
    }
}