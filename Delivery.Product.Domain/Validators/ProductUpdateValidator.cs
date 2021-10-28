using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement;
using FluentValidation;

namespace Delivery.Product.Domain.Validators
{
    public class ProductUpdateValidator : AbstractValidator<ProductManagementUpdateContract>
    {
        public ProductUpdateValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().NotNull().WithMessage("Product id must be provided.");
            RuleFor(x => x.ProductName).NotEmpty().NotNull().WithMessage("Product name must be provided.");
            RuleFor(x => x.Description).NotEmpty().NotNull().WithMessage("Description must be provided.");
            RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Unit price must be provided.");
            RuleFor(x => x.CategoryId).NotEmpty().NotNull().WithMessage("Category id must be provided.");
        }
    }
}