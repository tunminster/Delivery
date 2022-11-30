using Delivery.Database.Entities;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptionValues;

namespace Delivery.Managements.Domain.Converters;

public static class MeatOptionValueCreationConverter
{
    public static MeatOptionValue ConvertMeatOptionValueToEntity(this MeatOptionValueCreationContract meatOptionValueCreationContract, int meatOptionId)
    {
        var meatOptionValue = new MeatOptionValue
        {
            MeatOptionId = meatOptionId,
            OptionValueText = meatOptionValueCreationContract.MeatOptionValueText,
            AdditionalPrice = meatOptionValueCreationContract.AdditionalPrice
        };

        return meatOptionValue;
    }
}