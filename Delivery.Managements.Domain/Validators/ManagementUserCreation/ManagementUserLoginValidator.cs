using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.ManagementUserCreation
{
    public class ManagementUserLoginValidator : AbstractValidator<ManagementUserLoginContract>
    {
        public ManagementUserLoginValidator()
        {
            RuleFor(x => x.Username).NotEmpty().NotNull().WithMessage("Username must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be provided.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}