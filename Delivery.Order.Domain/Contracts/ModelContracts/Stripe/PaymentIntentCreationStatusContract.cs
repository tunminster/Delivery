namespace Delivery.Order.Domain.Contracts.ModelContracts.Stripe
{
    public class PaymentIntentCreationStatusContract
    {
        public PaymentIntentCreationStatusContract(string stripePaymentIntentId, string stripeClientSecret, string orderId)
        {
            StripePaymentIntentId = stripePaymentIntentId;
            StripeClientSecret = stripeClientSecret;
            OrderId = orderId;
        }
        
        public string StripePaymentIntentId { get;  }
        public string StripeClientSecret { get; }
        
        public string OrderId { get;  }
    }
}