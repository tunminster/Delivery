using Delivery.Managements.Domain.Contracts.V1.RestContracts.UpdateProfile;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.UpdateProfile
{
    public class UpdateProfileCreationValidator : AbstractValidator<UpdateProfileCreationContract>
    {
        public UpdateProfileCreationValidator()
        {
            RuleFor(x => x.Password).NotEmpty().NotNull().WithMessage("Password must be provided");
            RuleFor(x => x.ConfirmPassword).NotEmpty().NotNull().WithMessage("Confirm password must be provided");
            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match.");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password should be at least 6 characters");
        }
    }
}