using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.EmailVerification
{
    public class ManagementUserEmailVerificationValidator : AbstractValidator<ManagementUserEmailVerificationContract>
    {
        public ManagementUserEmailVerificationValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.Code).NotEmpty().NotNull().WithMessage("Code must be provided.");
        }
    }
}