using System.Collections.Generic;
using System.Linq;
using Delivery.Database.Entities;
using Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptions;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions;

namespace Delivery.Managements.Domain.Converters
{
    public static class MeatOptionCreationConverter
    {
        public static MeatOption ConvertMeatOptionToEntity(
            this MeatOptionCreationMessageContract meatOptionCreationMessageContract, int productId)
        {
            var meatOption = new MeatOption
            {
                ProductId = productId,
                ExternalId = meatOptionCreationMessageContract.MeatOptionId,
                OptionControl = meatOptionCreationMessageContract.OptionControlType,
                OptionText = meatOptionCreationMessageContract.MeatOptionText
            };

            if (!meatOptionCreationMessageContract.MeatOptionValues.Any()) return meatOption;
            meatOption.MeatOptionValues = new List<MeatOptionValue>();
            foreach (var item in meatOptionCreationMessageContract.MeatOptionValues)
            {
                meatOption.MeatOptionValues.Add(new MeatOptionValue{ OptionValueText = item.OptionValueText, AdditionalPrice = item.AdditionalPrice});
            }

            return meatOption;
        }

        public static MeatOptionCreationMessageContract ConvertToMeatOptionCreationMessageContract(
            this MeatOptionCreationContract meatOptionCreationContract, string externalId)
        {
            var meatOptionCreationMessageContract = new MeatOptionCreationMessageContract
            {
                ProductId = meatOptionCreationContract.ProductId,
                OptionControlType = meatOptionCreationContract.OptionControlType,
                MeatOptionText = meatOptionCreationContract.MeatOptionText,
                MeatOptionId = externalId,
                MeatOptionValues = meatOptionCreationContract.MeatOptionValues
            };
            return meatOptionCreationMessageContract;
        }
    }
}