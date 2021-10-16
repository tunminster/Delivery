using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.EmailVerification
{
    public class ManagementUserEmailVerificationRequestValidator : AbstractValidator<ManagementUserEmailVerificationRequestContract>
    {
        public ManagementUserEmailVerificationRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().NotNull().WithMessage("Full name must be provided.");
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}