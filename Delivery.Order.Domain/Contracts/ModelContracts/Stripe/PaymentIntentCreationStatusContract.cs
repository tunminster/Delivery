namespace Delivery.Order.Domain.Contracts.ModelContracts.Stripe
{
    public class PaymentIntentCreationStatusContract
    {
        public PaymentIntentCreationStatusContract(string stripePaymentIntentId, string stripeClientSecret, string orderId, string storeOwnerTransferId)
        {
            StripePaymentIntentId = stripePaymentIntentId;
            StripeClientSecret = stripeClientSecret;
            OrderId = orderId;
            StoreOwnerTransferId = storeOwnerTransferId;
        }
        
        public string StripePaymentIntentId { get;  }
        public string StripeClientSecret { get; }
        
        public string StoreOwnerTransferId { get; }
        
        public string OrderId { get;  }
    }
}