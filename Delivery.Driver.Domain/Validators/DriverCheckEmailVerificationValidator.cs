using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverCheckEmailVerificationValidator : AbstractValidator<DriverCheckEmailVerificationContract>
    {
        public DriverCheckEmailVerificationValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.Code).NotEmpty().NotNull().WithMessage("Code must be provided.");
        }
    }
}