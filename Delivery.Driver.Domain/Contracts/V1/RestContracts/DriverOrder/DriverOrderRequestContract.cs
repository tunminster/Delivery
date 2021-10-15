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
        ///  Store address
        /// <example>{{storeAddress}}</example>
        /// </summary>
        public string StoreAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Delivery Address
        /// <example>{{deliveryAddress}}</example>
        /// </summary>
        public string DeliveryAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery Fee
        ///  <example>{{deliveryFee}}</example>
        /// </summary>
        public int DeliveryFee { get; init; }
        
        /// <summary>
        ///  Tips
        /// <example>{{tips}}</example>
        /// </summary>
        public int Tips { get; init; }
    }
}