using System.Linq;
using Delivery.Database.Entities;
using Delivery.Domain.MeatOptions.Contracts.V1.RestContracts;

namespace Delivery.Domain.MeatOptions.Converters
{
    public static class MeatOptionConverter
    {
        public static MeatOptionContract ConvertToMeatOptionContract(this MeatOption meatOption)
        {
            var meatOptionContract = new MeatOptionContract
            {
                Id = meatOption.ExternalId,
                MeatOptionText = meatOption.OptionText,
                ProductId = meatOption.Product.ExternalId,
                OptionControlType = meatOption.OptionControl,
                MeatOptionValues = meatOption.MeatOptionValues
                    .Select(x => x.ConvertToMeatOptionValueContract(meatOption.ExternalId))
                    .ToList()
            };
            return meatOptionContract;
        }

        public static MeatOptionValueContract ConvertToMeatOptionValueContract(this MeatOptionValue meatOptionValue, string meatOptionId)
        {
            var meatOptionValueContract = new MeatOptionValueContract
            {
                Id = meatOptionValue.ExternalId,
                MeatOptionId = meatOptionId,
                OptionValueText = meatOptionValue.OptionValueText,
                AdditionalPrice = meatOptionValue.AdditionalPrice
            };
            return meatOptionValueContract;
        }
    }
}