using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderTotalAmountCreation
{
    public class StripeOrderTotalAmountCreationCommand
    {
        public StripeOrderTotalAmountCreationCommand(StripeOrderCreationContract stripeOrderCreationContract, OrderCreationStatusContract orderCreationStatusContract,
            string promotionCode, int promotionDiscount)
        {
            StripeOrderCreationContract = stripeOrderCreationContract;
            OrderCreationStatusContract = orderCreationStatusContract;
            PromotionCode = promotionCode;
            PromotionDiscount = promotionDiscount;
        }
        
        public StripeOrderCreationContract StripeOrderCreationContract { get; }
        
        public OrderCreationStatusContract OrderCreationStatusContract { get;  }
        
        public string PromotionCode { get; }
        
        public int PromotionDiscount { get;  }
    }
}