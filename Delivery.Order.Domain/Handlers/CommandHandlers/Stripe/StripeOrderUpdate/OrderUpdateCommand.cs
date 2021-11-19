using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate
{
    public class OrderUpdateCommand
    {
        public OrderUpdateCommand(StripeOrderUpdateContract stripeOrderUpdateContract, StripeOrderUpdateStatusContract stripeOrderUpdateStatusContract)
        {
            StripeOrderUpdateContract = stripeOrderUpdateContract;
            StripeOrderUpdateStatusContract = stripeOrderUpdateStatusContract;
        }
        
        public StripeOrderUpdateContract StripeOrderUpdateContract { get; }
        
        public StripeOrderUpdateStatusContract StripeOrderUpdateStatusContract { get;  }
    }
}