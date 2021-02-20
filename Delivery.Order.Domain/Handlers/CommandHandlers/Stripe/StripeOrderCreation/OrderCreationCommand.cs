using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderCreation
{
    public class OrderCreationCommand
    {
        public OrderCreationCommand(StripeOrderCreationContract stripeOrderCreationContract, OrderCreationStatusContract orderCreationStatusContract)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            OrderCreationStatusContract = orderCreationStatusContract;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public OrderCreationStatusContract OrderCreationStatusContract { get;  }
    }
}