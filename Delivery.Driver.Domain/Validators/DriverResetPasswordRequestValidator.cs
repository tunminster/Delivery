using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverResetPasswordVerification;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverResetPasswordRequestValidator : AbstractValidator<DriverResetPasswordRequestContract>
    {
        public DriverResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}