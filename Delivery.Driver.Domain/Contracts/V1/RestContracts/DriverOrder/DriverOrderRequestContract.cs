using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder
{
    /// <summary>
    ///  Driver order request contract
    /// </summary>
    public record DriverOrderRequestContract : NotificationDataContract
    {
        /// <summary>
        ///  ImageUri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store address
        /// </summary>
        /// <example>{{storeAddress}}</example>
        public string StoreAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Delivery Address
        /// </summary>
        /// <example>{{deliveryAddress}}</example>
        public string DeliveryAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery Fee
        /// </summary>
        ///  <example>{{deliveryFee}}</example>
        public int DeliveryFee { get; init; }
        
        /// <summary>
        ///  Tips
        /// </summary>
        /// <example>{{tips}}</example>
        public int Tips { get; init; }
    }
}