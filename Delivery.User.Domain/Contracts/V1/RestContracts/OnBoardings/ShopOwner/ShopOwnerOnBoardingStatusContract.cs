namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.ShopOwner
{
    /// <summary>
    ///  Shop owner on boarding status contract
    /// </summary>
    public record ShopOwnerOnBoardingStatusContract
    {
        /// <summary>
        ///  Account number
        /// </summary>
        /// <example>{{accountNumber}}</example>
        public string AccountNumber { get; init; } = string.Empty;
    }
}