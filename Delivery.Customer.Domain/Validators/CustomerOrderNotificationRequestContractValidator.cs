using Delivery.Customer.Domain.Contracts.V1.Enums;
using Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification;
using FluentValidation;

namespace Delivery.Customer.Domain.Validators
{
    public class CustomerOrderNotificationRequestContractValidator : AbstractValidator<CustomerOrderNotificationRequestContract>
    {
        public CustomerOrderNotificationRequestContractValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().NotNull().WithMessage("Order id must be provided.");
            RuleFor(x => x.Filter).NotEqual(OrderNotificationFilter.None).WithMessage("Filter must be provided.");
        }
    }
}