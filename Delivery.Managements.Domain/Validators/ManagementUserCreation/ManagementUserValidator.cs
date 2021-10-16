using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.ManagementUserCreation
{
    public class ManagementUserValidator : AbstractValidator<ManagementUserContract>
    {
        public ManagementUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}