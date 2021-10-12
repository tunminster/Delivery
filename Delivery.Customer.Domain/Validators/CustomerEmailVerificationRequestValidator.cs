using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Customer.Domain.Validators
{
    public class CustomerEmailVerificationRequestValidator : AbstractValidator<CustomerEmailVerificationRequestContract>
    {
        public CustomerEmailVerificationRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}