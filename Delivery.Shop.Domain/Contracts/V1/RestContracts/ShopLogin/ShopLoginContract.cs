namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopLogin
{
    /// <summary>
    ///  Shop login contract
    /// </summary>
    public record ShopLoginContract
    {
        /// <summary>
        ///  Username
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        ///  Password
        /// </summary>
        public string Password { get; init; } = string.Empty;
    }
}