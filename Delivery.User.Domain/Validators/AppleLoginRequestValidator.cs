using Delivery.User.Domain.Contracts.Apple;
using FluentValidation;

namespace Delivery.User.Domain.Validators
{
    public class AppleLoginRequestValidator : AbstractValidator<AppleLoginRequestContract>
    {
        public AppleLoginRequestValidator()
        {
            RuleFor(x => x.AuthorizationCode).NotNull().NotEmpty().WithMessage("Authorization code must be provided.");
            RuleFor(x => x.IdentityToken).NotNull().NotEmpty().WithMessage("Identity token must be provided.");
        }
    }
}