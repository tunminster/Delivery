using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverLoginValidator : AbstractValidator<DriverLoginContract>
    {
        public DriverLoginValidator()
        {
            RuleFor(x => x.Username).NotEmpty().NotNull().WithMessage("Username must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be provided.");
        }
    }
}