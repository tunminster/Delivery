using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Customer.Domain.Contracts.V1.Enums;
using Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Domain.Helpers;

namespace Delivery.Customer.Domain.Converters
{
    public static class CustomerPushNotificationContractConverter
    {
        public static CustomerOrderArrivedNotificationContract ConvertToCustomerOrderArrivedNotificationContract(
            this Delivery.Database.Entities.Order order, OrderNotificationFilter orderNotificationFilter)
        {
            var pushNotificationType = orderNotificationFilter switch
            {
                OrderNotificationFilter.Accept => PushNotificationType.CustomerOrderAccepted,
                OrderNotificationFilter.Arrived => PushNotificationType.CustomerOrderArrived,
                OrderNotificationFilter.Delivered => PushNotificationType.CustomerDelivered,
                _ => PushNotificationType.None
            };

            var customerOrderNotificationContract = new CustomerOrderArrivedNotificationContract
            {
                StoreAddress = order.Store.FormattedAddress?.Replace("+", ",") ?? string.Empty,
                StoreId = order.Store.ExternalId,
                DeliveryAddress = FormatAddressLinesHelper.FormatAddress(order.Address.AddressLine,
                    string.Empty, order.Address.City, string.Empty,
                    order.Address.Country, order.Address.PostCode),
                DeliveryFee = order.DeliveryFees,
                OrderId = order.ExternalId,
                PushNotificationType = pushNotificationType,
                OrderType = order.OrderType
            };
            return customerOrderNotificationContract;
        }
    }
}