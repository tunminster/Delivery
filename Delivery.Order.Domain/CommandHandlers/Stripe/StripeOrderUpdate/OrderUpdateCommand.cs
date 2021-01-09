using Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderUpdate
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