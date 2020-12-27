using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.StripePayment.Domain.Validators
{
    public class StripeAccountLinkCreationValidator : AbstractValidator<StripeAccountLinkCreationContract>
    {
        public StripeAccountLinkCreationValidator()
        {
            RuleFor(x => x.AccountId).NotNull().NotEmpty().WithMessage("Account id must be provided.");
            RuleFor(x => x.RefreshUrl).NotEmpty().WithMessage("Refresh url must be provided.");
            RuleFor(x => x.ReturnUrl).NotEmpty().WithMessage("Return url must be provided.");
            
        }
    }
}