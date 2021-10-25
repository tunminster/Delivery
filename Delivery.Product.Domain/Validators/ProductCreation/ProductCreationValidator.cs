using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement;
using FluentValidation;

namespace Delivery.Product.Domain.Validators.ProductCreation
{
    public class ProductCreationValidator : AbstractValidator<ProductManagementCreationContract>
    {
        public ProductCreationValidator()
        {
            RuleFor(x => x.ProductName).NotEmpty().NotNull().WithMessage("Product name must be provided.");
            RuleFor(x => x.Description).NotEmpty().NotNull().WithMessage("Description must be provided.");
            RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Unit price must be provided.");
            RuleFor(x => x.CategoryId).NotEmpty().NotNull().WithMessage("Category id must be provided.");
        }
    }
}