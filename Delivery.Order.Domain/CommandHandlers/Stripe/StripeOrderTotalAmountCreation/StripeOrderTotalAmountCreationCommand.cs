using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderTotalAmountCreation
{
    public class StripeOrderTotalAmountCreationCommand
    {
        public StripeOrderTotalAmountCreationCommand(StripeOrderCreationContract stripeOrderCreationContract, OrderCreationStatusContract orderCreationStatusContract)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            OrderCreationStatusContract = orderCreationStatusContract;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public OrderCreationStatusContract OrderCreationStatusContract { get;  }
    }
}