using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverOrderActionValidator : AbstractValidator<DriverOrderActionContract>
    {
        public DriverOrderActionValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().NotNull().WithMessage("Order id must be provided.");
            RuleFor(x => x.DriverOrderStatus).NotEqual(DriverOrderStatus.None).WithMessage("Status must be provided.");
        }
    }
}