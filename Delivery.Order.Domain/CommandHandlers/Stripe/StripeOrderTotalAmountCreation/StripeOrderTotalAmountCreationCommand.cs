using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderTotalAmountCreation
{
    public class StripeOrderTotalAmountCreationCommand
    {
        public StripeOrderTotalAmountCreationCommand(StripeOrderCreationContract stripeOrderCreationContract, OrderCreationStatus orderCreationStatus)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            OrderCreationStatus = orderCreationStatus;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public OrderCreationStatus OrderCreationStatus { get;  }
    }
}