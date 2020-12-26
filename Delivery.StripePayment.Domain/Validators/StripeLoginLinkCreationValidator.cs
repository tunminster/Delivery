using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.StripePayment.Domain.Validators
{
    public class StripeLoginLinkCreationValidator : AbstractValidator<StripeLoginLinkCreationContract>
    {
        public StripeLoginLinkCreationValidator()
        {
            RuleFor(x => x.AccountId).NotNull().NotEmpty().WithMessage("Account id must be provided.");
        }
    }
}