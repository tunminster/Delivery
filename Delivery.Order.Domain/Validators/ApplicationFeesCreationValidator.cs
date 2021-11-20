using Delivery.Order.Domain.Contracts.V1.RestContracts.ApplicationFees;
using FluentValidation;

namespace Delivery.Order.Domain.Validators
{
    public class ApplicationFeesCreationValidator : AbstractValidator<ApplicationFeesCreationContract>
    {
        public ApplicationFeesCreationValidator()
        {
            RuleFor(x => x.SubTotal).NotEmpty().NotNull().WithMessage("Subtotal must be provided.");
            RuleFor(x => x.SubTotal).GreaterThan(99).WithMessage("Subtotal must be greater than 99.");
        }
    }
}