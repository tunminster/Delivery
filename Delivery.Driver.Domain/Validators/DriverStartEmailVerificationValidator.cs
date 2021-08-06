using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators
{
    public class DriverStartEmailVerificationValidator : AbstractValidator<DriverStartEmailVerificationContract>
    {
        public DriverStartEmailVerificationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().NotNull().WithMessage("Driver full name must be provided.");
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}