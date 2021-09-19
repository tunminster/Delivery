using System;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Driver request contract for an order 
    /// </summary>
    public record ShopOrderDriverRequestContract
    {
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        // /// <summary>
        // ///  Store name
        // /// </summary>
        // /// <example>{{storeName}}</example>
        // public string StoreName { get; init; } = string.Empty;
        //
        // /// <summary>
        // ///  Store image uri
        // /// </summary>
        // /// <example>{{storeImageUri}}</example>
        // public string StoreImageUri { get; init; } = string.Empty;
        //
        // /// <summary>
        // ///  Store address
        // /// </summary>
        // /// <example>{{storeAddress}}</example>
        // public string StoreAddress { get; init; } = string.Empty;
        //
        // /// <summary>
        // ///  Delivery address
        // /// <example>{{deliveryAddress}}</example>
        // /// </summary>
        // public string DeliveryAddress { get; init; } = string.Empty;
        //
        // /// <summary>
        // ///  Delivery fee
        // /// <example>{{deliveryFee}}</example>
        // /// </summary>
        // public int DeliveryFee { get; init; }
        //
        // /// <summary>
        // ///  Tips
        // /// <example>{{deliveryTips}}</example>
        // /// </summary>
        // public int DeliveryTips { get; init; }
    }
}