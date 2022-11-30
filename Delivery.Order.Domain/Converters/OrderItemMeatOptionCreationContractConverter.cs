using System.Linq;
using Delivery.Database.Entities;
using Delivery.Order.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Converters
{
    public static class OrderItemMeatOptionCreationContractConverter
    {
        public static OrderItemMeatOption ConvertToOrderItemMeatOptionsEntity(this OrderItemMeatOptionCreationContract orderItemMeatOptionCreationContract)
        {
            var orderItemMeatOption = new OrderItemMeatOption
            {
                MeatOptionId = orderItemMeatOptionCreationContract.MeatOptionId,
                MeatOptionText = orderItemMeatOptionCreationContract.OptionText,
                OrderItemMeatOptionValues = orderItemMeatOptionCreationContract.MeatOptionValues
                    .Select(x => new OrderItemMeatOptionValue
                    {
                        MeatOptionValueId = x.MeatOptionValueId,
                        MeatOptionValueText = x.OptionValueText
                    }).ToList()
            };

            return orderItemMeatOption;
        }
    }
}