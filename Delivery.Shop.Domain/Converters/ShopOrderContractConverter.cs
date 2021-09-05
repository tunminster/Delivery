using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Database.Entities;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Microsoft.Graph;

namespace Delivery.Shop.Domain.Converters
{
    public static class ShopOrderContractConverter
    {
        public static List<ShopOrderContract> ConvertToContract(List<Database.Entities.Order> orders, List<Database.Entities.DriverOrder> driverOrders)
        {
            return orders.Select(item => new ShopOrderContract
                {
                    StoreId = item.Store.ExternalId,
                    OrderId = item.ExternalId,
                    OrderType = item.OrderType,
                    ShopOrderItems = ConvertToShopOrderItem(item.OrderItems.ToList()),
                    ShopOrderDriver = ConvertToShopOrderDriver(driverOrders.FirstOrDefault(x => x.OrderId == item.Id))
                })
                .ToList();
        }

        private static ShopOrderDriverContract? ConvertToShopOrderDriver(DriverOrder? driverOrder)
        {
            if (driverOrder == null) return null;
            var shopOrderDriverContract = new ShopOrderDriverContract
            {
                DriverId = driverOrder.Driver.ExternalId,
                ImageUri = driverOrder.Driver.ImageUri,
                Name = driverOrder.Driver.FullName
            };
            return shopOrderDriverContract;
        }

        private static List<ShopOrderItemContract> ConvertToShopOrderItem(List<Database.Entities.OrderItem> orderItems)
        {
            return orderItems
                .Select(item => new ShopOrderItemContract 
                    { ItemName = item.Product.ProductName, Count = item.Count, Price = item.Product.UnitPrice * item.Count }).ToList();
        }
    }
}