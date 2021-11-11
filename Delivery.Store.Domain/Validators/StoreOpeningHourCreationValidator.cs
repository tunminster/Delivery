using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using FluentValidation;

namespace Delivery.Store.Domain.Validators
{
    public class StoreOpeningHourCreationValidator : AbstractValidator<StoreOpeningHourContract>
    {
        public StoreOpeningHourCreationValidator()
        {
            RuleFor(x => x.Open).NotNull().NotEmpty().WithMessage("Open hour must be provided.");
            RuleFor(x => x.Close).NotNull().NotEmpty().WithMessage("Close hour must be provided.");
            RuleFor(x => x.DayOfWeek).NotNull().NotEmpty().WithMessage("Day of week must be provided.");
            RuleFor(x => x.TimeZone).NotNull().NotEmpty().WithMessage("Timezone must be provided.");
        }
    }
}