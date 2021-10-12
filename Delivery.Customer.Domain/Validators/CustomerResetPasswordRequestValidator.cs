using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Customer.Domain.Validators
{
    public class CustomerResetPasswordRequestValidator : AbstractValidator<CustomerResetPasswordRequestContract>
    {
        public CustomerResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}