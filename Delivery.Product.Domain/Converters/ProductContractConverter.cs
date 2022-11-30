using System.Linq;
using Delivery.Database.Entities;
using Delivery.Product.Domain.Contracts.V1.RestContracts;

namespace Delivery.Product.Domain.Converters
{
    public static class ProductContractConverter
    {
        public static ProductMeatOptionContract ConvertToProductMeatOptionContract(this MeatOption meatOption)
        {
            var productMeatOptionContract = new ProductMeatOptionContract
            {
                MeatOptionId = meatOption.Id,
                OptionControl = meatOption.OptionControl,
                OptionText = meatOption.OptionText,
                ProductMeatOptionValues = meatOption.MeatOptionValues
                    .Select(x => new ProductMeatOptionValueContract
                    {
                        MeatOptionValueId = x.Id,
                        OptionValueText = x.OptionValueText,
                        AdditionalPrice = x.AdditionalPrice
                    }).ToList()
            };
            return productMeatOptionContract;
        }
    }
}