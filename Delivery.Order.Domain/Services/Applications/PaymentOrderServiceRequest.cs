using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.Services.Applications
{
    public class PaymentOrderServiceRequest
    {
        public PaymentOrderServiceRequest(StripeOrderCreationContract stripeOrderCreationContract, string currencyCode)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            CurrencyCode = currencyCode;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public string CurrencyCode { get; }
    }
}