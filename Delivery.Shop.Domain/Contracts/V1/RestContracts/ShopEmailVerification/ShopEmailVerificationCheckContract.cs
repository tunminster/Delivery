namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification
{
    /// <summary>
    ///  Shop verification check contract
    /// </summary>
    public class ShopEmailVerificationCheckContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        ///  Verify code
        /// </summary>
        public string Code { get; init; } = string.Empty;
    }
}