namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification
{
    /// <summary>
    ///  Shop email verification contract
    /// </summary>
    public record ShopEmailVerificationContract
    {
        /// <summary>
        ///  Shop owner full name
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Shop owner email address
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;
    }
}