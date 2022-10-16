using System.Collections.Generic;
using System.Linq;
using Delivery.Database.Entities;
using Delivery.Domain.Helpers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverOrderDetailsContractConverter
    {
        public static DriverOrderDetailsContract ConvertToDriverOrderDetailsContract(this DriverOrder driverOrder)
        {
            var driverOrderDetailsContract = new DriverOrderDetailsContract
            {
                OrderId = driverOrder.Order.ExternalId,
                StoreId = driverOrder.Order.Store.ExternalId,
                StoreName = driverOrder.Order.Store.StoreName,
                StoreAddress = driverOrder.Order.Store.FormattedAddress ?? string.Empty,
                StoreLatitude = driverOrder.Order.Store.Latitude ?? 0,
                StoreLongitude = driverOrder.Order.Store.Longitude ?? 0,
                DeliveryAddress = FormatAddressLinesHelper.FormatAddress(driverOrder.Order.Address.AddressLine,
                    string.Empty, driverOrder.Order.Address.City, string.Empty,
                    driverOrder.Order.Address.Country, driverOrder.Order.Address.PostCode),
                DeliveryLatitude = driverOrder.Order.Address.Lat ?? 0,
                DeliveryLongitude = driverOrder.Order.Address.Lng ?? 0,
                DeliveryFee = driverOrder.Order.DeliveryFees,
                Tips = driverOrder.Order.DeliveryTips ?? 0,
                TotalAmount = driverOrder.Order.TotalAmount,
                OrderItems = ConvertToOrderItemContract(driverOrder.Order.OrderItems.ToList())
            };

            return driverOrderDetailsContract;
        }

        private static List<OrderDetailsItemContract> ConvertToOrderItemContract(this List<OrderItem> orderItems)
        {
            return orderItems.Select(item => new OrderDetailsItemContract
            {
                Name = item.Product.ProductName,
                ItemPrice = item.Product.UnitPrice
            }).ToList();
        }

        public static List<DriverOrderDetailsContract> ConvertToDriverOrderDetailsContract(
            this List<DriverOrder> driverOrders)
        {
            return driverOrders.Select(item => new DriverOrderDetailsContract
                {
                    OrderId = item.Order.ExternalId,
                    StoreId = item.Order.Store.ExternalId,
                    StoreName = item.Order.Store.StoreName,
                    StoreAddress = item.Order.Store.FormattedAddress ?? string.Empty,
                    StoreLatitude = item.Order.Store.Latitude ?? 0,
                    StoreLongitude = item.Order.Store.Longitude ?? 0,
                    DeliveryAddress = FormatAddressLinesHelper.FormatAddress(item.Order.Address.AddressLine, string.Empty, item.Order.Address.City, string.Empty, item.Order.Address.Country, item.Order.Address.PostCode),
                    DeliveryLatitude = item.Order.Address.Lat ?? 0,
                    DeliveryLongitude = item.Order.Address.Lng ?? 0,
                    DeliveryFee = item.Order.DeliveryFees,
                    Tips = 0,
                    TotalAmount = item.Order.TotalAmount,
                    OrderItems = ConvertToOrderItemContract(item.Order.OrderItems.ToList())
                })
                .ToList();
        }
    }
}