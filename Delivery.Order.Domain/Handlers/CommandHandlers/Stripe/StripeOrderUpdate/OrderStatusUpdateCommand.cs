using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrderUpdate;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate
{
    public record OrderStatusUpdateCommand(StripeUpdateOrderContract StripeUpdateOrderContract);
}