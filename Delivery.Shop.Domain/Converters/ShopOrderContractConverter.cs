using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Database.Entities;
using Delivery.Shop.Domain.Constants;
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
                    Subtotal = item.SubTotal,
                    Status = item.Status,
                    PlatformServiceFee = item.PlatformServiceFees,
                    DeliveryFee = item.DeliveryFees,
                    Tax = item.TaxFees,
                    TotalAmount = item.TotalAmount,
                    PreparationTime = item.PreparationTime ?? ShopConstant.DefaultPreparationTime,
                    PickupTime = item.PickupTime ?? DateTimeOffset.UtcNow.AddMinutes(ShopConstant.DefaultPreparationTime + ShopConstant.DefaultPickupMinutes),
                    ShopOrderItems = ConvertToShopOrderItem(item.OrderItems.ToList()),
                    IsPreparationCompleted = DateTimeOffset.UtcNow > item.InsertionDateTime.AddMinutes(ShopConstant.DefaultPreparationTime),
                    DateCreated = item.InsertionDateTime,
                    OrderAcceptedDateTime = item.OrderAcceptedDateTime,
                    DeliveryEstimatedDateTime = item.DeliveryEstimatedDateTime,
                    ShopOrderDeliveryAddress = item.Address != null ? new ShopOrderDeliveryAddress { AddressLine1 = item.Address.AddressLine ?? string.Empty, City = item.Address.City, PostalCode = item.Address.PostCode, Latitude = item.Address.Lat, Longitude = item.Address.Lng} : null,
                    ShopOrderDriver = driverOrders.Count > 0 ? ConvertToShopOrderDriver(driverOrders.FirstOrDefault(x => x.OrderId == item.Id)) : new ShopOrderDriverContract()
                })
                .ToList();
        }

        public static ShopOrderContract ConvertToContract(Order order, DriverOrder? driverOrder)
        {
            var shopOrderContract = new ShopOrderContract
            {
                StoreId = order.Store.ExternalId,
                OrderId = order.ExternalId,
                OrderType = order.OrderType,
                Subtotal = order.SubTotal,
                Status = order.Status,
                PlatformServiceFee = order.PlatformServiceFees,
                DeliveryFee = order.DeliveryFees,
                Tax = order.TaxFees,
                BusinessServiceFee = order.BusinessServiceFees,
                TotalAmount = order.TotalAmount,
                ShopOrderItems = ConvertToShopOrderItem(order.OrderItems.ToList()),
                OrderAcceptedDateTime = order.OrderAcceptedDateTime,
                DeliveryEstimatedDateTime = order.DeliveryEstimatedDateTime,
                ShopOrderDriver = driverOrder != null ? ConvertToShopOrderDriver(driverOrder) : new ShopOrderDriverContract()
            };

            return shopOrderContract;
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