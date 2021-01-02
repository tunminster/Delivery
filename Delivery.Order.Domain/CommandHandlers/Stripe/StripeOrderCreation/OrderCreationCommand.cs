using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderCreation
{
    public class OrderCreationCommand
    {
        public OrderCreationCommand(StripeOrderCreationContract stripeOrderCreationContract, OrderCreationStatus orderCreationStatus)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            OrderCreationStatus = orderCreationStatus;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public OrderCreationStatus OrderCreationStatus { get;  }
    }
}