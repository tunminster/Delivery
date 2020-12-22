using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderCreation
{
    public class OrderCreationCommand
    {
        public OrderCreationCommand(StripeOrderCreationContract stripeOrderCreationContract, string currencyCode)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            CurrencyCode = currencyCode;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public string CurrencyCode { get;  }
    }
}