using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Domain.Helpers;
using Delivery.Order.Domain.Constants;
using Delivery.Order.Domain.Contracts.V1.RestContracts.PushNotification;

namespace Delivery.Order.Domain.Converters
{
    public static class OrderCreatedPushNotificationContractConverter
    {
        public static OrderCreatedPushNotificationContract ConvertToContract(this Database.Entities.Order order)
        {
            var orderCreatedPushNotificationContract = new OrderCreatedPushNotificationContract
            {
                OrderId = order.ExternalId,
                StoreId = order.Store.ExternalId,
                StoreName = order.Store.StoreName,
                PushNotificationType = PushNotificationType.ShopNewOrder,
                OrderType = order.OrderType,
                Status = order.Status,
                ShopOrderItems = ConvertToShopOrderItem(order.OrderItems.ToList()),
                Subtotal = order.SubTotal,
                TotalAmount = order.TotalAmount,
                PlatformServiceFee = order.PlatformServiceFees,
                DeliveryFee = order.DeliveryFees,
                Tax = order.TaxFees,
                PreparationTime = order.PreparationTime ?? OrderConstant.DefaultPreparationTime,
                PickupTime = order.PickupTime ??
                             DateTimeOffset.UtcNow.AddMinutes(OrderConstant.DefaultPreparationTime +
                                                              OrderConstant.DefaultPickupMinutes),
                IsPreparationCompleted = DateTimeOffset.UtcNow >
                                         order.InsertionDateTime.AddMinutes(OrderConstant.DefaultPreparationTime),
                DateCreated = order.InsertionDateTime,
                StoreAddress = order.Store.FormattedAddress,
                DeliveryAddress = order.Address != null ? FormatAddressLinesHelper.FormatAddress(order.Address.AddressLine,
                    string.Empty, order.Address.City, string.Empty,
                    order.Address.Country, order.Address.PostCode) : string.Empty,
                ShopOrderDeliveryAddress = order.Address != null ? new OrderDeliveryAddressContract { AddressLine1 = order.Address.AddressLine ?? string.Empty, City = order.Address.City, PostalCode = order.Address.PostCode, Latitude = order.Address.Lat, Longitude = order.Address.Lng} : null,
            };
            return orderCreatedPushNotificationContract;
        }
        
        private static List<OrderCreatedItemContract> ConvertToShopOrderItem(List<Database.Entities.OrderItem> orderItems)
        {
            return orderItems
                .Select(item => new OrderCreatedItemContract 
                    { ItemName = item.Product.ProductName, Count = item.Count, Price = item.Product.UnitPrice * item.Count }).ToList();
        }
    }
}