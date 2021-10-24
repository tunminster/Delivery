using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverAssignment
{
    public class DriverOrderIndexCreationContractValidator : AbstractValidator<DriverOrderIndexCreationContract>
    {
        public DriverOrderIndexCreationContractValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().NotNull().WithMessage("Order id must be provided.");
            RuleFor(x => x.DriverId).NotEmpty().NotNull().WithMessage("Driver id must be provided.");
        }
    }
}