using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.PushNotification;
using Microsoft.Graph;

namespace Delivery.Shop.Domain.Converters.PushNotification
{
    public static class ShopOrderCompletePushNotificationConverter
    {
        public static ShopOrderCompletePushNotificationContract ConvertToShopOrderCompletePushNotificationContract(this Database.Entities.Order order)
        {
            var shopOrderCompletePushNotificationContract = new ShopOrderCompletePushNotificationContract
            {
                PushNotificationType = PushNotificationType.OrderCompleted,
                OrderId = order.ExternalId,
                StoreId = order.Store.ExternalId,
                StoreName = order.Store.StoreName
            };

            return shopOrderCompletePushNotificationContract;
        }
    }
}