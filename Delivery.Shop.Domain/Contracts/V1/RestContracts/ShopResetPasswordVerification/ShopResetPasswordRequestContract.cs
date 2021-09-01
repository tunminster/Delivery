namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification
{
    /// <summary>
    ///  Request password reset contract
    /// </summary>
    public record ShopResetPasswordRequestContract
    {
        /// <summary>
        ///  Reset email
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}