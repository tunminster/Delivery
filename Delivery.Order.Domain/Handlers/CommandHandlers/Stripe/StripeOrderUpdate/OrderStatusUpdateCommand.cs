using Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderUpdate
{
    public record OrderStatusUpdateCommand(StripeUpdateOrderContract StripeUpdateOrderContract);
}