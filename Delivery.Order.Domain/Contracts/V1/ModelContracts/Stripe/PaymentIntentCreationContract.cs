using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.V1.ModelContracts.Stripe
{
    /// <summary>
    ///  Stripe payment creation contract
    /// </summary>
    public record PaymentIntentCreationContract
    {
        public string PaymentMethod { get; set; }
        
        public int Amount { get; set; }
        
        public int Subtotal { get; set; }
        
        public string Currency { get; set; }
        
        public int ApplicationFeeAmount { get; set; }
        
        public string OrderId { get; set; }
        
        public string StoreConnectedStripeAccountId { get; set; }

        public string DriverConnectedStripeAccountId { get; set; } = string.Empty;
        
        public int DeliveryFeeAmount { get; set; }
        
        public int BusinessFeeAmount { get; set; }
        
        public int CustomerApplicationFeeAmount { get; set; }
        
        public int TaxFeeAmount { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(PaymentMethod)}: {PaymentMethod.Format()}," +
                   $"{nameof(Amount)}: {Amount.Format()}," +
                   $"{nameof(Currency)}: {Currency.Format()}," +
                   $"{nameof(ApplicationFeeAmount)}: {ApplicationFeeAmount.Format()}," +
                   $"{nameof(StoreConnectedStripeAccountId)} : {StoreConnectedStripeAccountId.Format()}";

        }
    }
}