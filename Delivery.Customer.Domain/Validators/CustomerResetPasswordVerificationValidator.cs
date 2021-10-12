using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Customer.Domain.Validators
{
    public class CustomerResetPasswordVerificationValidator : AbstractValidator<CustomerResetPasswordCreationContract>
    { 
        public CustomerResetPasswordVerificationValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
            RuleFor(x => x.Code).NotEmpty().NotNull().WithMessage("Code must be provided.");
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be provided");
            RuleFor(x => x.ConfirmPassword).NotEmpty().NotNull().WithMessage("Confirm password must be provided");
            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password should be at least 6 characters");
        }
    }
}