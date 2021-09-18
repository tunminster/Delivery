namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder
{
    /// <summary>
    ///  Get order details contract
    /// </summary>
    public record DriverOrderDetailsContract
    {
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Store id
        /// <example>{{storeId}}</example>
        /// </summary>
        public string StoreId { get; init; } = string.Empty;

        /// <summary>
        ///  Store name
        ///  <example>{{storeName}}</example>
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

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