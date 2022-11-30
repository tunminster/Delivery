using Delivery.Database.Enums;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions;
using FluentValidation;

namespace Delivery.Managements.Domain.Validators.MeatOptions
{
    public class MeatOptionCreationValidator : AbstractValidator<MeatOptionCreationContract>
    {
        public MeatOptionCreationValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().NotNull()
                .WithMessage($"{nameof(MeatOptionCreationContract.ProductId)} must be provided");
            RuleFor(x => x.MeatOptionText).NotEmpty().NotNull()
                .WithMessage($"{nameof(MeatOptionCreationContract.MeatOptionText)} must be provided");
            RuleFor(x => x.OptionControlType).NotEqual(OptionControlType.None).WithMessage(
                $"{nameof(MeatOptionCreationContract.OptionControlType)} must not be {nameof(OptionControlType.None)}");
        }
    }
}