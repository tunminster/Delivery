using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Customer.Domain.Validators
{
    public class CustomerEmailVerificationValidator : AbstractValidator<CustomerEmailVerificationContract>
    {
        public CustomerEmailVerificationValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.Code).NotEmpty().NotNull().WithMessage("Code must be provided.");
        }
    }
}