using Delivery.Azure.Library.Contracts.Extensions;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.StripePayment.Domain.Validators
{
    public class StripeAccountCreationValidator : AbstractValidator<StripeAccountCreationContract>
    {
        public StripeAccountCreationValidator()
        {
            RuleFor(x => x.StripeAccountType).WithValidEnum("Stripe account type needs to be provided.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email must be provided.");
        }
    }
}