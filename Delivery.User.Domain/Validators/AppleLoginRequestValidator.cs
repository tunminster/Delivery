using Delivery.User.Domain.Contracts.Apple;
using FluentValidation;

namespace Delivery.User.Domain.Validators
{
    public class AppleLoginRequestValidator : AbstractValidator<AppleLoginRequestContract>
    {
        public AppleLoginRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().WithMessage("Email name must be provided.");
            RuleFor(x => x.FamilyName).NotNull().NotEmpty().WithMessage("Family name must be provided.");
            RuleFor(x => x.GivenName).NotNull().NotEmpty().WithMessage("Given name must be provided.");
            RuleFor(x => x.AuthorizationCode).NotNull().NotEmpty().WithMessage("Authorization code must be provided.");
            RuleFor(x => x.IdentityToken).NotNull().NotEmpty().WithMessage("Identity token must be provided.");
        }
    }
}