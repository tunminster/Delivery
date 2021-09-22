namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile
{
    /// <summary>
    ///  Shop profile image creation contract
    /// </summary>
    public class ShopProfileImageCreationContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;

        /// <summary>
        ///  Image uri
        /// </summary>
        public string ImageUri { get; init; } = string.Empty;
    }
}