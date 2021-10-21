using Delivery.Category.Domain.Contracts.V1.RestContracts;
using FluentValidation;

namespace Delivery.Category.Domain.Validators.CategoryCreation
{
    public class CategoryCreationValidator : AbstractValidator<CategoryCreationContract>
    {
        public CategoryCreationValidator()
        {
            RuleFor(x => x.CategoryName).NotEmpty().NotNull().WithMessage("Category name must be provided.");
            RuleFor(x => x.Description).NotEmpty().NotNull().WithMessage("Description must be provided.");
        }
    }
}