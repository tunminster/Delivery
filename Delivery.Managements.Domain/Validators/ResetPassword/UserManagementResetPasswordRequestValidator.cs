using Delivery.Managements.Domain.Contracts.V1.RestContracts.ResetPassword;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.ResetPassword
{
    public class UserManagementResetPasswordRequestValidator : AbstractValidator<UserManagementResetPasswordRequestContract>
    {
        public UserManagementResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull().WithMessage("Email address must be provided.");
        }
    }
}