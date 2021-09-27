using System.Collections.Generic;
using System.Linq;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu;

namespace Delivery.Shop.Domain.Converters.ShopMenu
{
    public static class ShopMenuStatusContractConverter
    {
        public static List<ShopMenuStatusContract> ConvertToContract(this List<Database.Entities.Product> product)
        {
            var shopMenuStatusContractList = product.Select(x => new ShopMenuStatusContract
            {
                ProductId = x.ExternalId,
                ProductName = x.ProductName,
                ProductImage = x.ProductImageUrl,
                UnitPrice = x.UnitPrice,
                Status = x.IsActive
            }).ToList();

            return shopMenuStatusContractList;
        }
    }
}