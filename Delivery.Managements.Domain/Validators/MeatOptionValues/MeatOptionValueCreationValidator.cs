using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptionValues;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.MeatOptionValues
{
    public class MeatOptionValueCreationValidator : AbstractValidator<MeatOptionValueCreationContract>
    {
        public MeatOptionValueCreationValidator()
        {
            RuleFor(x => x.MeatOptionId).NotEmpty().NotNull().WithMessage($"{nameof(MeatOptionValueCreationContract.MeatOptionId)} must be provided");
            RuleFor(x => x.MeatOptionValueText).NotEmpty().NotNull().WithMessage($"{nameof(MeatOptionValueCreationContract.MeatOptionValueText)} must be provided");
        }
    }
}